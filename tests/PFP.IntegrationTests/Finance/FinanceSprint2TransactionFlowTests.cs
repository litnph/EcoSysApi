using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[CollectionDefinition("FinanceSprint2", DisableParallelization = true)]
public sealed class FinanceSprint2Collection;

[Collection("FinanceSprint2")]
public sealed class FinanceSprint2TransactionFlowTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint2TransactionFlowTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_direct_transaction_updates_balance()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        const long amount = 100;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "direct",
                amount,
                harness.SourceAId,
                harness.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null),
            FinanceApiWireJson.Web);

        var createBody = await createResp.Content.ReadAsStringAsync();
        Assert.True(createResp.StatusCode == HttpStatusCode.OK, $"Status {(int)createResp.StatusCode}: {createBody}");

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var source = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
        Assert.Equal(1000m - amount, source.Balance);
    }

    [Fact]
    public async Task Delete_deferred_transaction_on_open_statement_removes_item_and_recalculates_cycle()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var cardName = $"Delete duplicate card {Guid.NewGuid():N}";
        var createCardResponse = await client.PostAsJsonAsync(
            "api/v1/finance/sources",
            new
            {
                name = cardName,
                type = "creditCard",
                currency = "VND",
                creditLimit = 10_000_000L,
                statementDay = 20,
                paymentDueDay = 7,
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createCardResponse.StatusCode);

        Guid cardId;
        await using (var sourceScope = _fixture.Services.CreateAsyncScope())
        {
            var db = sourceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            cardId = await db.FinSources
                .Where(source => source.Name == cardName)
                .Select(source => source.Id)
                .SingleAsync();
        }

        var generateResponse = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = cardId },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, generateResponse.StatusCode);

        Guid cycleId;
        DateOnly transactionDate;
        await using (var cycleScope = _fixture.Services.CreateAsyncScope())
        {
            var db = cycleScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cycle = await db.FinBillingCycles
                .Where(row => row.SourceId == cardId)
                .Select(row => new { row.Id, row.PeriodEnd })
                .SingleAsync();
            cycleId = cycle.Id;
            transactionDate = cycle.PeriodEnd;
        }

        const long amount = 120_640;
        var createTransactionResponse = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "deferred",
                amount,
                cardId,
                harness.ExpenseCategoryId,
                transactionDate,
                "PAYOO-BACH HOA XANH",
                null,
                null),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, createTransactionResponse.StatusCode);
        var transactionId = await FinanceApiWireJson
            .ReadTransactionIdFromCreateResponseAsync(createTransactionResponse);

        var addItemResponse = await client.PostAsJsonAsync(
            $"api/v1/finance/billing-cycles/{cycleId}/items",
            new { transactionId },
            FinanceApiWireJson.Web);
        Assert.True(
            addItemResponse.StatusCode == HttpStatusCode.OK,
            await addItemResponse.Content.ReadAsStringAsync());

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"api/v1/finance/transactions/{transactionId}")
        {
            Content = JsonContent.Create(
                new { reason = "Duplicate statement import", expectedVersion = 1 },
                options: FinanceApiWireJson.Web),
        };
        var deleteResponse = await client.SendAsync(deleteRequest);
        Assert.True(
            deleteResponse.StatusCode == HttpStatusCode.OK,
            await deleteResponse.Content.ReadAsStringAsync());

        await using var assertScope = _fixture.Services.CreateAsyncScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletedTransaction = await assertDb.FinTransactions
            .IgnoreQueryFilters()
            .SingleAsync(row => row.Id == transactionId);
        var statementItem = await assertDb.FinBillingCycleItems
            .SingleAsync(row => row.TransactionId == transactionId);
        var cycleTotal = await assertDb.FinBillingCycles
            .Where(row => row.Id == cycleId)
            .Select(row => row.TotalAmount)
            .SingleAsync();
        var reversalExists = await assertDb.FinTransactions
            .IgnoreQueryFilters()
            .AnyAsync(
                row => row.Type == TransactionType.Reversal
                       && row.RefTxnId == transactionId);

        Assert.True(deletedTransaction.IsDeleted);
        Assert.NotNull(statementItem.RemovedAt);
        Assert.Equal(0m, cycleTotal);
        Assert.True(reversalExists);
    }

    [Fact]
    public async Task Recalculate_credit_card_counts_legacy_paid_installments_without_transaction_links()
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var card = new FinSource
        {
            Name = $"Legacy paid card {Guid.NewGuid():N}",
            Type = SourceType.CreditCard,
            Balance = 0m,
            Currency = "VND",
            CreditLimit = 35_000_000m,
            StatementDay = 20,
            PaymentDueDay = 15,
        };
        var charge = new FinTransaction
        {
            Type = TransactionType.Deferred,
            Status = TxnStatus.TransferredToInstallment,
            Amount = 7_791_000m,
            Currency = "VND",
            TxnDate = new DateOnly(2025, 9, 19),
            SourceId = card.Id,
            Description = "Legacy installment principal",
        };
        var plan = new FinInstallmentPlan
        {
            OriginalTxnId = charge.Id,
            SourceId = card.Id,
            TotalAmount = charge.Amount,
            TotalMonths = 12,
            MonthlyAmount = 649_250m,
            StartDate = charge.TxnDate,
            Status = InstallmentStatus.Completed,
        };

        db.FinSources.Add(card);
        db.FinTransactions.Add(charge);
        db.FinInstallmentPlans.Add(plan);

        for (var installmentNumber = 1; installmentNumber <= 12; installmentNumber++)
        {
            var statementDate = new DateOnly(2025, 9, 20).AddMonths(installmentNumber - 1);
            db.FinInstallmentPays.Add(new FinInstallmentPay
            {
                PlanId = plan.Id,
                InstallmentNumber = installmentNumber,
                StatementDate = statementDate,
                DueDate = statementDate.AddDays(15),
                Amount = 649_250m,
                PaidAmount = 649_250m,
                Status = InstallmentPayStatus.Paid,
                PaidAt = statementDate.AddDays(15).ToDateTime(TimeOnly.MinValue),
                TxnId = null,
            });
        }

        foreach (var statementDate in new[]
                 {
                     new DateOnly(2026, 6, 20),
                     new DateOnly(2026, 7, 20),
                     new DateOnly(2026, 8, 20),
                 })
        {
            db.FinBillingCycles.Add(new FinBillingCycle
            {
                SourceId = card.Id,
                Name = $"Statement {statementDate:yyyy-MM}",
                PeriodStart = statementDate.AddMonths(-1),
                PeriodEnd = statementDate.AddDays(-1),
                StatementDate = statementDate,
                PaymentDueDate = statementDate.AddDays(15),
                TotalAmount = 649_250m,
                PaidAmount = 649_250m,
                Status = BillingCycleStatus.Paid,
                PaidAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();

        var calculator = scope.ServiceProvider.GetRequiredService<IBalanceCalculator>();
        var computed = await calculator.PreviewAsync(card.Id);

        Assert.Equal(0m, computed);
    }

    [Fact]
    public async Task Recalculate_credit_card_uses_paid_status_instead_of_payment_amount_or_link()
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var card = new FinSource
        {
            Name = $"Status-only paid card {Guid.NewGuid():N}",
            Type = SourceType.CreditCard,
            Balance = 900m,
            Currency = "VND",
            CreditLimit = 10_000m,
            StatementDay = 20,
            PaymentDueDay = 15,
        };
        var charge = new FinTransaction
        {
            Type = TransactionType.Deferred,
            Status = TxnStatus.TransferredToInstallment,
            Amount = 1_000m,
            Currency = "VND",
            TxnDate = new DateOnly(2026, 1, 10),
            SourceId = card.Id,
            Description = "Status-only installment principal",
        };
        var plan = new FinInstallmentPlan
        {
            OriginalTxnId = charge.Id,
            SourceId = card.Id,
            TotalAmount = charge.Amount,
            TotalMonths = 10,
            MonthlyAmount = 100m,
            StartDate = charge.TxnDate,
            Status = InstallmentStatus.Active,
        };

        db.FinSources.Add(card);
        db.FinTransactions.Add(charge);
        db.FinInstallmentPlans.Add(plan);
        db.FinInstallmentPays.Add(new FinInstallmentPay
        {
            PlanId = plan.Id,
            InstallmentNumber = 1,
            StatementDate = new DateOnly(2026, 1, 20),
            DueDate = new DateOnly(2026, 2, 4),
            Amount = 100m,
            PaidAmount = 0m,
            Status = InstallmentPayStatus.Paid,
            TxnId = null,
        });
        await db.SaveChangesAsync();

        var calculator = scope.ServiceProvider.GetRequiredService<IBalanceCalculator>();
        var computed = await calculator.PreviewAsync(card.Id);

        Assert.Equal(900m, computed);
    }
}
