using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[CollectionDefinition("ApprovedBusinessRules", DisableParallelization = true)]
public sealed class ApprovedBusinessRulesCollection;

[Collection("ApprovedBusinessRules")]
public sealed class ApprovedBusinessRuleApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public ApprovedBusinessRuleApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Import_preview_rolls_back_commit_is_atomic_and_retries_are_idempotent()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var firstKey = Guid.NewGuid();
        var secondKey = Guid.NewGuid();
        var items = new[]
        {
            ImportItem(firstKey, harness, 100),
            ImportItem(secondKey, harness, 200),
        };

        var preview = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/preview",
            new { items },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        using (var previewJson = await ReadJsonAsync(preview))
            Assert.True(previewJson.RootElement.GetProperty("data").GetProperty("isValid").GetBoolean());
        Assert.Equal(0, await CountImportedRowsAsync(firstKey, secondKey));

        var commit = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit",
            new { items, allowPartial = false },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, commit.StatusCode);
        Assert.Equal(2, await CountImportedRowsAsync(firstKey, secondKey));

        var retry = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit",
            new { items, allowPartial = false },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal(2, await CountImportedRowsAsync(firstKey, secondKey));

        var conflictingItems = new[]
        {
            ImportItem(firstKey, harness, 999),
        };
        var conflict = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit",
            new { items = conflictingItems, allowPartial = false },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        var rollbackKey = Guid.NewGuid();
        var invalidKey = Guid.NewGuid();
        var atomicFailure = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit",
            new
            {
                items = new[]
                {
                    ImportItem(rollbackKey, harness, 50),
                    new
                    {
                        type = "direct",
                        amount = 50,
                        sourceId = harness.SourceAId,
                        categoryId = Guid.NewGuid(),
                        txnDate = FinanceBusinessCalendar.Today,
                        description = "Invalid import row",
                        clientRequestId = invalidKey,
                    },
                },
                allowPartial = false,
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.BadRequest, atomicFailure.StatusCode);
        Assert.Equal(0, await CountImportedRowsAsync(rollbackKey, invalidKey));
    }

    [Fact]
    public async Task Stale_source_update_returns_conflict()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var detail = await client.GetAsync($"api/v1/finance/sources/{harness.SourceAId}");
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        using var detailJson = await ReadJsonAsync(detail);
        var version = detailJson.RootElement
            .GetProperty("data")
            .GetProperty("source")
            .GetProperty("version")
            .GetInt32();

        var firstUpdate = await client.PutAsJsonAsync(
            $"api/v1/finance/sources/{harness.SourceAId}",
            SourceUpdateBody("Wallet updated", version),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        var staleUpdate = await client.PutAsJsonAsync(
            $"api/v1/finance/sources/{harness.SourceAId}",
            SourceUpdateBody("Stale overwrite", version),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
    }

    [Fact]
    public async Task New_transaction_in_a_closed_month_is_created_unassigned_and_can_be_deleted()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 10_000m, 500m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        DateOnly transactionDate;
        Guid closedPeriodId;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var occupiedPeriods = await db.FinMonthlyPeriods
                .AsNoTracking()
                .Select(period => new { period.Year, period.Month })
                .ToListAsync();
            transactionDate = Enumerable
                .Range(2000, FinanceBusinessCalendar.Today.Year - 2000)
                .SelectMany(year => Enumerable.Range(1, 12).Select(month => new DateOnly(year, month, 1)))
                .First(candidate => occupiedPeriods.All(
                    period => period.Year != candidate.Year || period.Month != candidate.Month));

            var closedPeriod = new FinMonthlyPeriod
            {
                Year = transactionDate.Year,
                Month = transactionDate.Month,
                Status = PeriodStatus.Closed,
                ClosedAt = DateTime.UtcNow,
            };
            db.FinMonthlyPeriods.Add(closedPeriod);
            await db.SaveChangesAsync();
            closedPeriodId = closedPeriod.Id;
        }

        var response = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "direct",
                100,
                harness.SourceAId,
                harness.ExpenseCategoryId,
                transactionDate,
                null,
                closedPeriodId,
                null),
            FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var transactionId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(response);

        await using var verificationScope = _fixture.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var monthlyPeriodId = await verificationDb.FinTransactions
            .AsNoTracking()
            .Where(transaction => transaction.Id == transactionId)
            .Select(transaction => transaction.MonthlyPeriodId)
            .SingleAsync();
        Assert.Null(monthlyPeriodId);

        var deleteResponse = await client.SendAsync(new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/v1/finance/transactions/{transactionId}")
        {
            Content = JsonContent.Create(new { reason = "Delete transaction dated in a closed month" }),
        });
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var isDeleted = await verificationDb.FinTransactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(transaction => transaction.Id == transactionId)
            .Select(transaction => transaction.IsDeleted)
            .SingleAsync();
        Assert.True(isDeleted);
    }

    [Fact]
    public async Task Upload_limit_cannot_be_raised_by_the_client()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 1_000m, 500m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("transaction"), "entity_type");
        form.Add(new StringContent(harness.SourceAId.ToString()), "entity_id");
        form.Add(new StringContent("50"), "max_file_size_mb");
        using var file = new ByteArrayContent([0x89, 0x50, 0x4e, 0x47]);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "proof.png");

        var response = await client.PostAsync("api/v1/files/upload", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("server limit", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Monthly_report_exposes_complete_per_currency_groups_without_consolidation()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 10_000m, 500m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        Guid usdSourceId;
        DateOnly reportDate;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var occupiedPeriods = await db.FinMonthlyPeriods
                .AsNoTracking()
                .Select(period => new { period.Year, period.Month })
                .ToListAsync();
            reportDate = Enumerable
                .Range(2000, FinanceBusinessCalendar.Today.Year - 2000)
                .SelectMany(year => Enumerable.Range(1, 12).Select(month => new DateOnly(year, month, 1)))
                .First(candidate => occupiedPeriods.All(
                    period => period.Year != candidate.Year || period.Month != candidate.Month));

            var usdSource = new FinSource
            {
                Name = $"USD wallet {Guid.NewGuid():N}",
                Type = SourceType.BankAccount,
                Balance = 1_000m,
                Currency = "USD",
                SortOrder = 20,
            };
            db.FinSources.Add(usdSource);
            await db.SaveChangesAsync();
            usdSourceId = usdSource.Id;
        }

        // With the default reporting day (1), a target month contains direct
        // transactions from the first of the previous month up to (but not
        // including) the first of the target month.
        var directTransactionDate = reportDate.AddMonths(-1);

        foreach (var row in new[]
                 {
                     new { SourceId = harness.SourceAId, Amount = 100L },
                     new { SourceId = usdSourceId, Amount = 25L },
                 })
        {
            var response = await client.PostAsJsonAsync(
                "api/v1/finance/transactions",
                new
                {
                    type = "direct",
                    amount = row.Amount,
                    sourceId = row.SourceId,
                    categoryId = harness.ExpenseCategoryId,
                    txnDate = directTransactionDate,
                    description = "Currency partition test",
                    clientRequestId = Guid.NewGuid(),
                },
                FinanceApiWireJson.Web);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var createReport = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = reportDate.Year, month = reportDate.Month },
            FinanceApiWireJson.Web);
        Assert.True(
            createReport.StatusCode == HttpStatusCode.OK,
            $"Expected monthly report creation to succeed, but received {(int)createReport.StatusCode}: {await createReport.Content.ReadAsStringAsync()}");

        var report = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportDate.Year}/{reportDate.Month}/report");
        Assert.Equal(HttpStatusCode.OK, report.StatusCode);
        using var reportJson = await ReadJsonAsync(report);
        var data = reportJson.RootElement.GetProperty("data");
        Assert.Equal("open", data.GetProperty("status").GetString());
        var payload = data.GetProperty("report");
        var groups = payload.GetProperty("currencyGroups").EnumerateArray().ToArray();
        var currencies = groups.Select(group => group.GetProperty("currency").GetString()).ToHashSet();

        Assert.Contains("VND", currencies);
        Assert.Contains("USD", currencies);
        Assert.False(payload.GetProperty("metadata").GetProperty("consolidatedTotalsAvailable").GetBoolean());
    }

    [Fact]
    public async Task Closing_month_completes_asset_transactions_but_not_credit_card_transactions()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 10_000m, 500m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        DateOnly reportDate;
        Guid creditCardId;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var occupiedPeriods = await db.FinMonthlyPeriods
                .AsNoTracking()
                .Select(period => new { period.Year, period.Month })
                .ToListAsync();
            reportDate = Enumerable
                .Range(2000, FinanceBusinessCalendar.Today.Year - 2000)
                .SelectMany(year => Enumerable.Range(1, 12).Select(month => new DateOnly(year, month, 1)))
                .First(candidate => occupiedPeriods.All(
                    period => period.Year != candidate.Year || period.Month != candidate.Month));

            var creditCard = new FinSource
            {
                Name = $"Monthly close card {Guid.NewGuid():N}",
                Type = SourceType.CreditCard,
                Balance = 0m,
                Currency = "VND",
                CreditLimit = 50_000m,
                StatementDay = 10,
                PaymentDueDay = 25,
                SortOrder = 30,
            };
            db.FinSources.Add(creditCard);
            await db.SaveChangesAsync();
            creditCardId = creditCard.Id;
        }

        var directResponse = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new
            {
                type = "direct",
                amount = 100L,
                sourceId = harness.SourceAId,
                categoryId = harness.ExpenseCategoryId,
                txnDate = reportDate,
                description = "Monthly close asset transaction",
                clientRequestId = Guid.NewGuid(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, directResponse.StatusCode);
        var directTransactionId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(directResponse);

        var deferredResponse = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new
            {
                type = "deferred",
                amount = 200L,
                sourceId = creditCardId,
                categoryId = harness.ExpenseCategoryId,
                txnDate = reportDate,
                description = "Monthly close credit-card transaction",
                clientRequestId = Guid.NewGuid(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, deferredResponse.StatusCode);
        var deferredTransactionId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(deferredResponse);

        var createReport = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = reportDate.Year, month = reportDate.Month },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createReport.StatusCode);

        var closeReport = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/close",
            new { year = reportDate.Year, month = reportDate.Month },
            FinanceApiWireJson.Web);
        Assert.True(
            closeReport.StatusCode == HttpStatusCode.OK,
            $"Expected monthly close to succeed, but received {(int)closeReport.StatusCode}: {await closeReport.Content.ReadAsStringAsync()}");

        await using var verificationScope = _fixture.Services.CreateAsyncScope();
        var verificationDb = verificationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var statuses = await verificationDb.FinTransactions
            .AsNoTracking()
            .Where(transaction => transaction.Id == directTransactionId
                || transaction.Id == deferredTransactionId)
            .ToDictionaryAsync(transaction => transaction.Id, transaction => transaction.Status);

        Assert.Equal(TxnStatus.Completed, statuses[directTransactionId]);
        Assert.Equal(TxnStatus.New, statuses[deferredTransactionId]);
    }

    [Fact]
    public async Task Upper_boundary_profile_setting_recalculates_an_open_monthly_report_window()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 10_000m, 500m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        DateOnly reportMonth;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var occupiedPeriods = await db.FinMonthlyPeriods
                .AsNoTracking()
                .Select(period => new { period.Year, period.Month })
                .ToListAsync();
            reportMonth = Enumerable
                .Range(2000, FinanceBusinessCalendar.Today.Year - 2000)
                .SelectMany(year => Enumerable.Range(1, 12)
                    .Select(month => new DateOnly(year, month, 1)))
                .First(candidate => occupiedPeriods.All(
                    period => period.Year != candidate.Year || period.Month != candidate.Month));
        }

        async Task SetPeriodModeAsync(string mode)
        {
            var response = await client.PutAsJsonAsync(
                "api/v1/user/profile",
                new
                {
                    fullName = "Integration User",
                    displayName = (string?)null,
                    phoneNumber = (string?)null,
                    dateOfBirth = (string?)null,
                    languageCode = "vi",
                    timezone = "Asia/Ho_Chi_Minh",
                    dateFormat = "dd/MM/yyyy",
                    theme = "system",
                    monthlyReportDay = 1,
                    monthlyReportPeriodMode = mode,
                },
                FinanceApiWireJson.Web);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var profileJson = await ReadJsonAsync(response);
            Assert.Equal(
                mode,
                profileJson.RootElement
                    .GetProperty("data")
                    .GetProperty("profile")
                    .GetProperty("monthlyReportPeriodMode")
                    .GetString());
        }

        await SetPeriodModeAsync("lowerBoundary");

        var transactionResponse = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new
            {
                type = "direct",
                amount = 654L,
                sourceId = harness.SourceAId,
                categoryId = harness.ExpenseCategoryId,
                txnDate = reportMonth.AddDays(5),
                description = "Upper boundary report transaction",
                clientRequestId = Guid.NewGuid(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, transactionResponse.StatusCode);
        var transactionId = await FinanceApiWireJson
            .ReadTransactionIdFromCreateResponseAsync(transactionResponse);

        var createReport = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = reportMonth.Year, month = reportMonth.Month },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createReport.StatusCode);

        var lowerReportResponse = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/report");
        Assert.Equal(HttpStatusCode.OK, lowerReportResponse.StatusCode);
        using (var lowerReportJson = await ReadJsonAsync(lowerReportResponse))
        {
            var lowerReport = lowerReportJson.RootElement.GetProperty("data").GetProperty("report");
            Assert.Equal(
                "lowerBoundary",
                lowerReport.GetProperty("metadata").GetProperty("monthlyReportPeriodMode").GetString());
            Assert.DoesNotContain(
                lowerReport.GetProperty("directExpenses").GetProperty("items").EnumerateArray(),
                item => item.GetProperty("id").GetGuid() == transactionId);
        }

        await SetPeriodModeAsync("upperBoundary");
        var refreshReport = await client.PostAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/refresh",
            content: null);
        Assert.Equal(HttpStatusCode.OK, refreshReport.StatusCode);

        var upperReportResponse = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/report");
        Assert.Equal(HttpStatusCode.OK, upperReportResponse.StatusCode);
        using var reportJson = await ReadJsonAsync(upperReportResponse);
        var report = reportJson.RootElement.GetProperty("data").GetProperty("report");
        var metadata = report.GetProperty("metadata");
        Assert.Equal("upperBoundary", metadata.GetProperty("monthlyReportPeriodMode").GetString());
        Assert.Equal(
            reportMonth,
            DateOnly.FromDateTime(metadata.GetProperty("reportingPeriodStart").GetDateTime()));
        Assert.Equal(
            reportMonth.AddMonths(1),
            DateOnly.FromDateTime(metadata.GetProperty("reportingPeriodEnd").GetDateTime()));
        Assert.Contains(
            report.GetProperty("directExpenses").GetProperty("items").EnumerateArray(),
            item => item.GetProperty("id").GetGuid() == transactionId);
    }

    [Fact]
    public async Task Direct_transaction_can_be_added_after_report_rule_change_and_recreation()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, 10_000m, 500m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        DateOnly reportMonth;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var occupiedPeriods = await db.FinMonthlyPeriods
                .AsNoTracking()
                .Select(period => new { period.Year, period.Month })
                .ToListAsync();
            reportMonth = Enumerable
                .Range(2000, FinanceBusinessCalendar.Today.Year - 2000)
                .SelectMany(year => Enumerable.Range(1, 12)
                    .Select(month => new DateOnly(year, month, 1)))
                .First(candidate => occupiedPeriods.All(
                    period => period.Year != candidate.Year || period.Month != candidate.Month));
        }

        async Task SetReportDayAsync(int day)
        {
            var response = await client.PutAsJsonAsync(
                "api/v1/user/profile",
                new
                {
                    fullName = "Integration User",
                    displayName = (string?)null,
                    phoneNumber = (string?)null,
                    dateOfBirth = (string?)null,
                    languageCode = "vi",
                    timezone = "Asia/Ho_Chi_Minh",
                    dateFormat = "dd/MM/yyyy",
                    theme = "system",
                    monthlyReportDay = day,
                },
                FinanceApiWireJson.Web);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await SetReportDayAsync(20);
        var transactionResponse = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new
            {
                type = "direct",
                amount = 321L,
                sourceId = harness.SourceAId,
                categoryId = harness.ExpenseCategoryId,
                txnDate = reportMonth.AddDays(9),
                description = "Manual monthly report membership",
                clientRequestId = Guid.NewGuid(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, transactionResponse.StatusCode);
        var transactionId = await FinanceApiWireJson
            .ReadTransactionIdFromCreateResponseAsync(transactionResponse);

        var createWithDayTwenty = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = reportMonth.Year, month = reportMonth.Month },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createWithDayTwenty.StatusCode);

        await SetReportDayAsync(1);
        var deleteResponse = await client.DeleteAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var recreateWithDayOne = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = reportMonth.Year, month = reportMonth.Month },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, recreateWithDayOne.StatusCode);

        var addableResponse = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/addable-transactions");
        Assert.Equal(HttpStatusCode.OK, addableResponse.StatusCode);
        using (var addableJson = await ReadJsonAsync(addableResponse))
        {
            var candidateIds = addableJson.RootElement
                .GetProperty("data")
                .GetProperty("items")
                .EnumerateArray()
                .Select(item => item.GetProperty("id").GetGuid())
                .ToList();
            Assert.Contains(transactionId, candidateIds);
        }

        var addResponse = await client.PostAsJsonAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/items",
            new { transactionId },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, addResponse.StatusCode);

        var reportResponse = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/report");
        Assert.Equal(HttpStatusCode.OK, reportResponse.StatusCode);
        using (var reportJson = await ReadJsonAsync(reportResponse))
        {
            var directIds = reportJson.RootElement
                .GetProperty("data")
                .GetProperty("report")
                .GetProperty("directExpenses")
                .GetProperty("items")
                .EnumerateArray()
                .Select(item => item.GetProperty("id").GetGuid())
                .ToList();
            Assert.Contains(transactionId, directIds);
        }

        var candidatesAfterAdd = await client.GetAsync(
            $"api/v1/finance/monthly-periods/{reportMonth.Year}/{reportMonth.Month}/addable-transactions");
        candidatesAfterAdd.EnsureSuccessStatusCode();
        using var afterAddJson = await ReadJsonAsync(candidatesAfterAdd);
        Assert.DoesNotContain(
            afterAddJson.RootElement
                .GetProperty("data")
                .GetProperty("items")
                .EnumerateArray()
                .Select(item => item.GetProperty("id").GetGuid()),
            id => id == transactionId);
    }

    private static object ImportItem(Guid key, FinanceTestHarness.FinanceHarness harness, long amount) =>
        new
        {
            type = "direct",
            amount,
            sourceId = harness.SourceAId,
            categoryId = harness.ExpenseCategoryId,
            txnDate = FinanceBusinessCalendar.Today,
            description = "Atomic import row",
            clientRequestId = key,
        };

    private static object SourceUpdateBody(string name, int expectedVersion) =>
        new
        {
            name,
            type = "bankAccount",
            creditLimit = (long?)null,
            statementDay = (int?)null,
            paymentDueDay = (int?)null,
            minInstallmentAmt = (long?)null,
            currency = "VND",
            icon = (string?)null,
            color = (string?)null,
            sortOrder = 0,
            expectedVersion,
        };

    private async Task<int> CountImportedRowsAsync(params Guid[] keys)
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.FinTransactions.AsNoTracking().CountAsync(row =>
            row.ClientRequestId != null && keys.Contains(row.ClientRequestId.Value));
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
        await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
}
