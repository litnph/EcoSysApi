using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Auth;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

/// <summary>Sprint 3 Task 7 — Installment plan flows (test group 2).</summary>
[Collection("FinanceSprint3")]
public sealed class FinanceSprint3InstallmentTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint3InstallmentTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Test_2_1_CreateInstallmentPlan_zero_percent_interest_with_conversion_fee()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var (cycleId, origTxnId) = await OpenCycleAndCreateDeferredAsync(client, h, 10_000);

        var planResp = await client.PostAsJsonAsync(
            "api/v1/finance/installment-plans",
            new
            {
                originalTxnId = origTxnId,
                totalMonths = 6,
                interestRate = 0m,
                conversionFeeRate = 1.5m,
            },
            FinanceApiWireJson.Web);
        planResp.EnsureSuccessStatusCode();
        var planId = await FinanceApiWireJson.ReadInstallmentPlanIdFromCreateResponseAsync(planResp);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var plan = await db.FinInstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == planId);
        var pays = await db.FinInstallmentPays.AsNoTracking()
            .Where(p => p.PlanId == planId)
            .OrderBy(p => p.InstallmentNumber)
            .ToListAsync();

        Assert.Equal(InstallmentStatus.Active, plan.Status);
        Assert.Equal(6, plan.TotalMonths);
        Assert.Equal(6, pays.Count);
        Assert.Equal(ConversionFeeStatus.Pending, plan.ConversionFeeStatus);
        Assert.Equal(150m, plan.ConversionFeeAmount ?? 0m);
    }

    [Fact]
    public async Task Test_2_2_ProcessConversionFee_emits_fee_in_cycle_and_marks_plan_billed()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var (cycleId, origTxnId) = await OpenCycleAndCreateDeferredAsync(client, h, 10_000);

        var planResp = await client.PostAsJsonAsync(
            "api/v1/finance/installment-plans",
            new
            {
                originalTxnId = origTxnId,
                totalMonths = 6,
                interestRate = 0m,
                conversionFeeRate = 1.5m,
            },
            FinanceApiWireJson.Web);
        planResp.EnsureSuccessStatusCode();
        var planId = await FinanceApiWireJson.ReadInstallmentPlanIdFromCreateResponseAsync(planResp);

        await FinanceSprint3BillingCycleTests.RunProcessConversionFeeAsync(_fixture, cycleId);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var plan = await db.FinInstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == planId);
        Assert.Equal(ConversionFeeStatus.Billed, plan.ConversionFeeStatus);
        Assert.NotNull(plan.ConversionFeeTxnId);

        var feeTxn = await db.FinTransactions.AsNoTracking().SingleAsync(t => t.Id == plan.ConversionFeeTxnId);
        Assert.Equal(TransactionType.Deferred, feeTxn.Type);
        Assert.Equal(cycleId, feeTxn.BillingCycleId);
        Assert.Equal(planId, feeTxn.InstallmentPlanId);
    }

    [Fact]
    public async Task Test_2_3_RecordInstallment_payment_marks_pay_paid_and_debits_bank()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var (cycleId, origTxnId) = await OpenCycleAndCreateDeferredAsync(client, h, 300);

        var planResp = await client.PostAsJsonAsync(
            "api/v1/finance/installment-plans",
            new
            {
                originalTxnId = origTxnId,
                totalMonths = 3,
                interestRate = 0m,
                conversionFeeRate = 1.5m,
            },
            FinanceApiWireJson.Web);
        planResp.EnsureSuccessStatusCode();
        var planId = await FinanceApiWireJson.ReadInstallmentPlanIdFromCreateResponseAsync(planResp);

        var bankBefore = await GetBankBalanceAsync(_fixture, h.BankSourceId);
        var payBefore = await GetFirstPayAmountAsync(_fixture, planId);

        var payResp = await client.PostAsJsonAsync(
            $"api/v1/finance/installment-plans/{planId}/pays/1/payment",
            new { paymentSourceId = h.BankSourceId },
            FinanceApiWireJson.Web);
        payResp.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var row = await db.FinInstallmentPays.AsNoTracking()
            .SingleAsync(p => p.PlanId == planId && p.InstallmentNumber == 1);
        Assert.Equal(InstallmentPayStatus.Paid, row.Status);
        var bankAfter = await db.FinSources.AsNoTracking().Select(s => new { s.Id, s.Balance })
            .SingleAsync(s => s.Id == h.BankSourceId);
        Assert.Equal(bankBefore - payBefore, bankAfter.Balance);
    }

    [Fact]
    public async Task Test_2_4_Paying_all_installments_completes_plan()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var (_, origTxnId) = await OpenCycleAndCreateDeferredAsync(client, h, 300);

        var planResp = await client.PostAsJsonAsync(
            "api/v1/finance/installment-plans",
            new
            {
                originalTxnId = origTxnId,
                totalMonths = 3,
                interestRate = 0m,
                conversionFeeRate = 1.5m,
            },
            FinanceApiWireJson.Web);
        planResp.EnsureSuccessStatusCode();
        var planId = await FinanceApiWireJson.ReadInstallmentPlanIdFromCreateResponseAsync(planResp);

        await MarkAllPaysDueAsync(_fixture, planId);

        for (var n = 1; n <= 3; n++)
        {
            var r = await client.PostAsJsonAsync(
                $"api/v1/finance/installment-plans/{planId}/pays/{n}/payment",
                new { paymentSourceId = h.BankSourceId },
                FinanceApiWireJson.Web);
            r.EnsureSuccessStatusCode();
        }

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var plan = await db.FinInstallmentPlans.AsNoTracking().SingleAsync(p => p.Id == planId);
        Assert.Equal(InstallmentStatus.Completed, plan.Status);
    }

    private static async Task<(Guid CycleId, Guid DeferredTxnId)> OpenCycleAndCreateDeferredAsync(
        HttpClient client,
        CreditCardHarness h,
        long deferredAmount)
    {
        var genResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = h.CreditCardSourceId },
            FinanceApiWireJson.Web);
        genResp.EnsureSuccessStatusCode();
        var cycleId = await FinanceApiWireJson.ReadBillingCycleIdFromGenerateResponseAsync(genResp);

        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var defResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "deferred",
                deferredAmount,
                h.CreditCardSourceId,
                h.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null,
                null,
                cycleId),
            FinanceApiWireJson.Web);
        defResp.EnsureSuccessStatusCode();
        var txnId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(defResp);
        return (cycleId, txnId);
    }

    private static async Task<decimal> GetBankBalanceAsync(IntegrationTestFixture fixture, Guid bankId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.FinSources.AsNoTracking().Where(s => s.Id == bankId).Select(s => s.Balance)
            .SingleAsync();
    }

    private static async Task<decimal> GetFirstPayAmountAsync(IntegrationTestFixture fixture, Guid planId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.FinInstallmentPays.AsNoTracking()
            .Where(p => p.PlanId == planId && p.InstallmentNumber == 1)
            .Select(p => p.Amount)
            .SingleAsync();
    }

    private static async Task MarkAllPaysDueAsync(IntegrationTestFixture fixture, Guid planId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pays = await db.FinInstallmentPays.Where(p => p.PlanId == planId).ToListAsync();
        foreach (var p in pays)
            p.Status = InstallmentPayStatus.Due;
        await db.SaveChangesAsync();
    }

    private static async Task<CreditCardHarness> RegisterUserSeedCreditCardScenarioAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var email = $"it-plan-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Installment User" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        var (accessToken, _, spaceId) = await AuthApiWire.ReadRegisterPayloadAsync(regResp);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var smodule = new SpaceModule
        {
            SpaceId = spaceId,
            ModuleCode = ModuleCode.Finance,
            IsEnabled = true,
            EnabledAt = DateTime.UtcNow,
        };
        db.SpaceModules.Add(smodule);
        await db.SaveChangesAsync();

        var expense = new FinCategory
        {
            SmoduleId = smodule.Id,
            Name = "Card spend",
            Code = "exp-" + Guid.NewGuid().ToString("N")[..16],
            Kind = CategoryKind.Expense,
            Depth = 0,
            SortOrder = 0,
            IsDefault = true,
            IsSystem = false,
        };
        db.FinCategories.Add(expense);

        var creditCard = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = "Plan Card",
            Type = SourceType.CreditCard,
            Balance = 0,
            Currency = "VND",
            StatementDay = 15,
            PaymentDueDay = 25,
            MinInstallmentAmt = 100m,
            SortOrder = 0,
        };
        var bank = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = "Checking",
            Type = SourceType.BankAccount,
            Balance = 100_000m,
            Currency = "VND",
            SortOrder = 1,
        };
        db.FinSources.AddRange(creditCard, bank);
        await db.SaveChangesAsync();

        return new CreditCardHarness(
            smodule.Id,
            creditCard.Id,
            bank.Id,
            expense.Id,
            accessToken);
    }

    private sealed record CreditCardHarness(
        Guid SmoduleId,
        Guid CreditCardSourceId,
        Guid BankSourceId,
        Guid ExpenseCategoryId,
        string AccessToken);
}
