using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[Collection("FinanceWorkflow")]
public sealed class BudgetNotificationApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    public BudgetNotificationApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PFP.Infrastructure.Persistence.AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Minimum_target_round_trips_projects_status_and_can_be_changed_to_maximum()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, client, sourceABalance: 10_000_000m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        async Task<HttpResponseMessage> SaveAsync(string targetMode) =>
            await client.PutAsJsonAsync(
                $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
                new
                {
                    isEnabled = true,
                    budgetAmount = 2_000_000L,
                    currency = "VND",
                    warningCount = 0,
                    warningThresholds = Array.Empty<decimal>(),
                    targetMode,
                },
                FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, (await SaveAsync("minimum")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client,
            owner,
            PFP.Application.Common.FinanceBusinessCalendar.Today,
            1_500_000)).StatusCode);

        var below = await ReadBudgetAsync(client, owner.ExpenseCategoryId);
        Assert.Equal("minimum", below.GetProperty("targetMode").GetString());
        Assert.Equal("belowTarget", below.GetProperty("status").GetString());
        Assert.Equal(75m, below.GetProperty("utilizationPercent").GetDecimal());
        Assert.Equal(500_000m, below.GetProperty("remainingAmount").GetDecimal());

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client,
            owner,
            PFP.Application.Common.FinanceBusinessCalendar.Today,
            500_000)).StatusCode);
        var achieved = await ReadBudgetAsync(client, owner.ExpenseCategoryId);
        Assert.Equal("targetAchieved", achieved.GetProperty("status").GetString());
        Assert.Equal(100m, achieved.GetProperty("utilizationPercent").GetDecimal());
        Assert.Equal(0m, achieved.GetProperty("remainingAmount").GetDecimal());

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client,
            owner,
            PFP.Application.Common.FinanceBusinessCalendar.Today,
            500_000)).StatusCode);
        var exceeded = await ReadBudgetAsync(client, owner.ExpenseCategoryId);
        Assert.Equal("targetExceeded", exceeded.GetProperty("status").GetString());
        Assert.Equal(125m, exceeded.GetProperty("utilizationPercent").GetDecimal());
        Assert.Equal(0m, exceeded.GetProperty("remainingAmount").GetDecimal());
        Assert.Equal("targetExceeded", (await ReadNotificationsAsync(client)).Items[0].Kind);

        Assert.Equal(HttpStatusCode.OK, (await SaveAsync("maximum")).StatusCode);
        var maximum = await ReadBudgetAsync(client, owner.ExpenseCategoryId);
        Assert.Equal("maximum", maximum.GetProperty("targetMode").GetString());
        Assert.Equal("exceeded", maximum.GetProperty("status").GetString());
        Assert.Equal(2, maximum.GetProperty("configurationVersion").GetInt32());
    }

    [Fact]
    public async Task Missing_mode_defaults_to_maximum_and_invalid_wire_value_is_rejected()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        var legacyRequest = await client.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 5_000_000L,
                currency = "VND",
                warningCount = 0,
                warningThresholds = Array.Empty<decimal>(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, legacyRequest.StatusCode);
        Assert.Equal(
            "maximum",
            (await ReadBudgetAsync(client, owner.ExpenseCategoryId))
                .GetProperty("targetMode")
                .GetString());

        var invalid = await client.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 5_000_000L,
                currency = "VND",
                warningCount = 0,
                warningThresholds = Array.Empty<decimal>(),
                targetMode = "unsupported",
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Budget_alerts_are_highest_only_idempotent_persistent_and_user_owned()
    {
        using var ownerClient = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, ownerClient, sourceABalance: 10_000m);
        ownerClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        var budgetResponse = await ownerClient.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 100L,
                currency = "VND",
                warningCount = 3,
                warningThresholds = new[] { 25m, 50m, 75m },
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, budgetResponse.StatusCode);

        var today = PFP.Application.Common.FinanceBusinessCalendar.Today;
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(ownerClient, owner, today, 20)).StatusCode);
        var belowThreshold = await ReadNotificationsAsync(ownerClient);
        Assert.Empty(belowThreshold.Items);

        var jumpResponse = await CreateDirectAsync(ownerClient, owner, today, 60);
        Assert.Equal(HttpStatusCode.OK, jumpResponse.StatusCode);
        var jumpedTransactionId = await ReadCreatedTransactionIdAsync(jumpResponse);

        var firstList = await ReadNotificationsAsync(ownerClient);
        Assert.Equal(1, firstList.UnreadCount);
        var warning = Assert.Single(firstList.Items);
        Assert.Equal("budgetWarning", warning.Kind);
        Assert.Equal(75m, warning.ThresholdPercent);

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(ownerClient, owner, today, 5)).StatusCode);
        var repeatedList = await ReadNotificationsAsync(ownerClient);
        Assert.Single(repeatedList.Items);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/v1/finance/transactions/{jumpedTransactionId}")
        {
            Content = JsonContent.Create(
                new { reason = "Budget alert idempotency check", expectedVersion = 1 },
                options: FinanceApiWireJson.Web),
        };
        var deleteResponse = await ownerClient.SendAsync(deleteRequest);
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(ownerClient, owner, today, 60)).StatusCode);
        Assert.Single((await ReadNotificationsAsync(ownerClient)).Items);

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(ownerClient, owner, today, 30)).StatusCode);
        var exceededList = await ReadNotificationsAsync(ownerClient);
        Assert.Equal(2, exceededList.Items.Count);
        Assert.Equal("budgetExceeded", exceededList.Items[0].Kind);
        Assert.Equal(115m, exceededList.Items[0].SpentAmount);

        var budgetsResponse = await ownerClient.GetAsync("api/v1/finance/category-budgets");
        budgetsResponse.EnsureSuccessStatusCode();
        await using (var budgetStream = await budgetsResponse.Content.ReadAsStreamAsync())
        using (var budgetDocument = await JsonDocument.ParseAsync(budgetStream))
        {
            var budgetItems = budgetDocument.RootElement
                .GetProperty("data")
                .GetProperty("budgets")
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();
            var budget = Assert.Single(
                budgetItems,
                item => item.GetProperty("categoryId").GetGuid() == owner.ExpenseCategoryId);
            Assert.Equal("exceeded", budget.GetProperty("status").GetString());
            Assert.Equal(115m, budget.GetProperty("utilizationPercent").GetDecimal());
            Assert.Equal(-15m, budget.GetProperty("remainingAmount").GetDecimal());
        }

        var readResponse = await ownerClient.PutAsync(
            $"api/v1/notifications/{exceededList.Items[0].Id}/read", null);
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        using var otherClient = _fixture.CreateClient();
        var other = await FinanceTestHarness.SeedAndLoginAsync(_fixture, otherClient);
        otherClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", other.AccessToken);
        var forbiddenRead = await otherClient.PutAsync(
            $"api/v1/notifications/{warning.Id}/read", null);
        Assert.Equal(HttpStatusCode.NotFound, forbiddenRead.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(ownerClient, owner, today, 10)).StatusCode);
        var finalList = await ReadNotificationsAsync(ownerClient);
        Assert.Equal(2, finalList.Items.Count);

        var markAllResponse = await ownerClient.PutAsync("api/v1/notifications/read-all", null);
        Assert.Equal(HttpStatusCode.OK, markAllResponse.StatusCode);
        Assert.Equal(0, (await ReadNotificationsAsync(ownerClient)).UnreadCount);
    }

    [Fact]
    public async Task Monthly_report_and_budget_projection_use_the_saved_direct_transaction_window()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, client, sourceABalance: 10_000m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        var profileResponse = await client.PutAsJsonAsync(
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
                monthlyReportDay = 20,
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client, owner, new DateOnly(2026, 7, 19), 10)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client, owner, new DateOnly(2026, 7, 20), 20)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client, owner, new DateOnly(2026, 8, 19), 30)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(
            client, owner, new DateOnly(2026, 8, 20), 40)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await CreateIncomeAsync(
            client, owner, new DateOnly(2026, 7, 19), 100)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateIncomeAsync(
            client, owner, new DateOnly(2026, 7, 20), 200)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateIncomeAsync(
            client, owner, new DateOnly(2026, 8, 19), 300)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateIncomeAsync(
            client, owner, new DateOnly(2026, 8, 20), 400)).StatusCode);

        var createReport = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = 2026, month = 8 },
            FinanceApiWireJson.Web);
        Assert.True(
            createReport.StatusCode == HttpStatusCode.OK,
            $"Expected report creation to succeed, but received {(int)createReport.StatusCode}: {await createReport.Content.ReadAsStringAsync()}");

        var budgetResponse = await client.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 100L,
                currency = "VND",
                warningCount = 0,
                warningThresholds = Array.Empty<decimal>(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, budgetResponse.StatusCode);

        var reportResponse = await client.GetAsync(
            "api/v1/finance/monthly-periods/2026/8/report");
        reportResponse.EnsureSuccessStatusCode();
        await using var stream = await reportResponse.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var report = document.RootElement.GetProperty("data").GetProperty("report");
        var metadata = report.GetProperty("metadata");
        Assert.Equal("2026-07-20", metadata.GetProperty("reportingPeriodStart").GetString());
        Assert.Equal("2026-08-20", metadata.GetProperty("reportingPeriodEnd").GetString());
        Assert.Equal(20, metadata.GetProperty("monthlyReportDay").GetInt32());
        Assert.Equal(
            "monthly-report-v4-income-reporting-cycle",
            metadata.GetProperty("formulaVersion").GetString());
        Assert.Equal(500m, report.GetProperty("summary").GetProperty("totalIncome").GetDecimal());

        var dailyIncome = report.GetProperty("dailyBreakdown")
            .EnumerateArray()
            .ToDictionary(
                item => item.GetProperty("date").GetString()!,
                item => item.GetProperty("income").GetDecimal());
        Assert.Equal(200m, dailyIncome["2026-07-20"]);
        Assert.Equal(300m, dailyIncome["2026-08-19"]);
        Assert.Equal(0m, dailyIncome["2026-08-20"]);

        var categoryItems = report.GetProperty("directExpenses").GetProperty("items")
            .EnumerateArray()
            .Where(item => item.GetProperty("categoryId").GetGuid() == owner.ExpenseCategoryId)
            .ToList();
        Assert.Equal(2, categoryItems.Count);
        Assert.Contains(categoryItems, item => item.GetProperty("txnDate").GetString() == "2026-07-20");
        Assert.Contains(categoryItems, item => item.GetProperty("txnDate").GetString() == "2026-08-19");

        var utilizationItems = report.GetProperty("budgetUtilizations")
            .EnumerateArray()
            .ToArray();
        var utilization = Assert.Single(
            utilizationItems,
            item => item.GetProperty("categoryId").GetGuid() == owner.ExpenseCategoryId);
        Assert.Equal(50m, utilization.GetProperty("spentAmount").GetDecimal());
        Assert.Equal(50m, utilization.GetProperty("utilizationPercent").GetDecimal());
    }

    [Fact]
    public async Task Transaction_owned_by_an_earlier_report_is_excluded_from_an_overlapping_later_report()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, client, sourceABalance: 10_000m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        async Task SaveReportDayAsync(int reportDay)
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
                    monthlyReportDay = reportDay,
                },
                FinanceApiWireJson.Web);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await SaveReportDayAsync(21);
        var august = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = 2025, month = 8 },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, august.StatusCode);

        Guid augustPeriodId;
        using (var periodScope = _fixture.Services.CreateScope())
        {
            var periodDb = periodScope.ServiceProvider
                .GetRequiredService<PFP.Infrastructure.Persistence.AppDbContext>();
            augustPeriodId = await periodDb.FinMonthlyPeriods
                .AsNoTracking()
                .Where(period => period.Year == 2025 && period.Month == 8)
                .Select(period => period.Id)
                .SingleAsync();
        }

        var createTransaction = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "direct",
                117,
                owner.SourceAId,
                owner.ExpenseCategoryId,
                new DateOnly(2025, 8, 20),
                null,
                augustPeriodId,
                null),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createTransaction.StatusCode);
        var transactionId = await ReadCreatedTransactionIdAsync(createTransaction);

        using (var unassignedScope = _fixture.Services.CreateScope())
        {
            var unassignedDb = unassignedScope.ServiceProvider
                .GetRequiredService<PFP.Infrastructure.Persistence.AppDbContext>();
            Assert.Null(await unassignedDb.FinTransactions
                .AsNoTracking()
                .Where(item => item.Id == transactionId)
                .Select(item => item.MonthlyPeriodId)
                .SingleAsync());
        }

        var refreshAugust = await client.PostAsync(
            "api/v1/finance/monthly-periods/2025/8/refresh",
            content: null);
        Assert.Equal(HttpStatusCode.OK, refreshAugust.StatusCode);
        Assert.Contains(transactionId, await ReadDirectTransactionIdsAsync(refreshAugust));

        await SaveReportDayAsync(20);
        var augustAfterPreferenceChange = await client.GetAsync(
            "api/v1/finance/monthly-periods/2025/8/report");
        Assert.Equal(HttpStatusCode.OK, augustAfterPreferenceChange.StatusCode);
        Assert.Contains(
            transactionId,
            await ReadDirectTransactionIdsAsync(augustAfterPreferenceChange));

        var september = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/reports",
            new { year = 2025, month = 9 },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, september.StatusCode);
        Assert.DoesNotContain(transactionId, await ReadDirectTransactionIdsAsync(september));

        var refreshSeptember = await client.PostAsync(
            "api/v1/finance/monthly-periods/2025/9/refresh",
            content: null);
        Assert.Equal(HttpStatusCode.OK, refreshSeptember.StatusCode);
        Assert.DoesNotContain(transactionId, await ReadDirectTransactionIdsAsync(refreshSeptember));

        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PFP.Infrastructure.Persistence.AppDbContext>();
        var transaction = await db.FinTransactions
            .AsNoTracking()
            .SingleAsync(item => item.Id == transactionId);
        Assert.Equal(augustPeriodId, transaction.MonthlyPeriodId);
    }

    [Fact]
    public async Task Budget_configuration_change_emits_only_the_highest_current_state_alert()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, client, sourceABalance: 10_000m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);
        var today = PFP.Application.Common.FinanceBusinessCalendar.Today;

        Assert.Equal(HttpStatusCode.OK, (await CreateDirectAsync(client, owner, today, 80)).StatusCode);
        Assert.Empty((await ReadNotificationsAsync(client)).Items);

        async Task<HttpResponseMessage> SaveBudgetAsync(long amount) =>
            await client.PutAsJsonAsync(
                $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
                new
                {
                    isEnabled = true,
                    budgetAmount = amount,
                    currency = "VND",
                    warningCount = 3,
                    warningThresholds = new[] { 25m, 50m, 75m },
                },
                FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, (await SaveBudgetAsync(100)).StatusCode);
        var configured = await ReadNotificationsAsync(client);
        var warning = Assert.Single(configured.Items);
        Assert.Equal("budgetWarning", warning.Kind);
        Assert.Equal(75m, warning.ThresholdPercent);

        Assert.Equal(HttpStatusCode.OK, (await SaveBudgetAsync(50)).StatusCode);
        var reconfigured = await ReadNotificationsAsync(client);
        Assert.Equal(2, reconfigured.Items.Count);
        Assert.Equal("budgetExceeded", reconfigured.Items[0].Kind);
    }

    [Fact]
    public async Task Invalid_budget_is_rejected_and_exact_limit_emits_reached_alert()
    {
        using var client = _fixture.CreateClient();
        var owner = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture, client, sourceABalance: 10_000m);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", owner.AccessToken);

        var invalid = await client.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 0L,
                currency = "VND",
                warningCount = 2,
                warningThresholds = new[] { 75m, 25m },
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var configured = await client.PutAsJsonAsync(
            $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
            new
            {
                isEnabled = true,
                budgetAmount = 100L,
                currency = "VND",
                warningCount = 0,
                warningThresholds = Array.Empty<decimal>(),
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, configured.StatusCode);
        Assert.Equal(1, await ReadBudgetConfigurationVersionAsync(configured));

        async Task<HttpResponseMessage> SaveAsync(long amount) =>
            await client.PutAsJsonAsync(
                $"api/v1/finance/category-budgets/{owner.ExpenseCategoryId}",
                new
                {
                    isEnabled = true,
                    budgetAmount = amount,
                    currency = "VND",
                    warningCount = 0,
                    warningThresholds = Array.Empty<decimal>(),
                },
                FinanceApiWireJson.Web);

        var concurrent = await Task.WhenAll(SaveAsync(110), SaveAsync(120));
        Assert.All(concurrent, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
        var versions = new[]
        {
            await ReadBudgetConfigurationVersionAsync(concurrent[0]),
            await ReadBudgetConfigurationVersionAsync(concurrent[1]),
        };
        Assert.Equal(new[] { 2, 3 }, versions.OrderBy(version => version));

        var exactLimit = await SaveAsync(100);
        Assert.Equal(HttpStatusCode.OK, exactLimit.StatusCode);
        Assert.Equal(4, await ReadBudgetConfigurationVersionAsync(exactLimit));

        var spend = await CreateDirectAsync(
            client,
            owner,
            PFP.Application.Common.FinanceBusinessCalendar.Today,
            100);
        Assert.Equal(HttpStatusCode.OK, spend.StatusCode);

        var notifications = await ReadNotificationsAsync(client);
        var reached = Assert.Single(notifications.Items);
        Assert.Equal("budgetReached", reached.Kind);
        Assert.Equal(100m, reached.SpentAmount);
    }

    private static Task<HttpResponseMessage> CreateDirectAsync(
        HttpClient client,
        FinanceTestHarness.FinanceHarness harness,
        DateOnly date,
        long amount) =>
        client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "direct", amount, harness.SourceAId, harness.ExpenseCategoryId,
                date, null, null, null),
            FinanceApiWireJson.Web);

    private static Task<HttpResponseMessage> CreateIncomeAsync(
        HttpClient client,
        FinanceTestHarness.FinanceHarness harness,
        DateOnly date,
        long amount) =>
        client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "income", amount, harness.SourceAId, harness.IncomeCategoryId,
                date, null, null, null),
            FinanceApiWireJson.Web);

    private static async Task<NotificationList> ReadNotificationsAsync(HttpClient client)
    {
        var response = await client.GetAsync("api/v1/notifications");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        var data = document.RootElement.GetProperty("data");
        var items = data.GetProperty("items")
            .EnumerateArray()
            .Select(item => new NotificationItem(
                item.GetProperty("id").GetGuid(),
                item.GetProperty("kind").GetString()!,
                item.TryGetProperty("spentAmount", out var spent) && spent.ValueKind != JsonValueKind.Null
                    ? spent.GetDecimal()
                    : null,
                item.TryGetProperty("thresholdPercent", out var threshold) && threshold.ValueKind != JsonValueKind.Null
                    ? threshold.GetDecimal()
                    : null))
            .ToList();
        return new NotificationList(data.GetProperty("unreadCount").GetInt32(), items);
    }

    private static async Task<Guid> ReadCreatedTransactionIdAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement
            .GetProperty("data")
            .GetProperty("transaction")
            .GetProperty("id")
            .GetGuid();
    }

    private static async Task<IReadOnlyList<Guid>> ReadDirectTransactionIdsAsync(
        HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement
            .GetProperty("data")
            .GetProperty("report")
            .GetProperty("directExpenses")
            .GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .ToList();
    }

    private static async Task<int> ReadBudgetConfigurationVersionAsync(
        HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement
            .GetProperty("data")
            .GetProperty("configurationVersion")
            .GetInt32();
    }

    private static async Task<JsonElement> ReadBudgetAsync(HttpClient client, Guid categoryId)
    {
        var response = await client.GetAsync("api/v1/finance/category-budgets");
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement
            .GetProperty("data")
            .GetProperty("budgets")
            .GetProperty("items")
            .EnumerateArray()
            .Single(item => item.GetProperty("categoryId").GetGuid() == categoryId)
            .Clone();
    }

    private sealed record NotificationList(int UnreadCount, IReadOnlyList<NotificationItem> Items);
    private sealed record NotificationItem(Guid Id, string Kind, decimal? SpentAmount, decimal? ThresholdPercent);
}
