using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Features.InstallmentPlans.Commands.ProcessConversionFee;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Auth;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

/// <summary>Sprint 3 Task 7 — Billing cycle + close month integration flows (test group 1).</summary>
[CollectionDefinition("FinanceSprint3", DisableParallelization = true)]
public sealed class FinanceSprint3CollectionDefinition;

[Collection("FinanceSprint3")]
public sealed class FinanceSprint3BillingCycleTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint3BillingCycleTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Test_1_1_GenerateBillingCycle_opens_cycle_with_expected_dates()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var genResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = h.CreditCardSourceId },
            FinanceApiWireJson.Web);
        genResp.EnsureSuccessStatusCode();

        var expected = BillingCycleDateMath.ExpectedForStatementDay(15, 25);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cycle = await db.FinBillingCycles.AsNoTracking().SingleAsync(c => c.SourceId == h.CreditCardSourceId);

        Assert.Equal(BillingCycleStatus.Open, cycle.Status);
        Assert.Equal(expected.PeriodStart, cycle.PeriodStart);
        Assert.Equal(expected.PeriodEnd, cycle.PeriodEnd);
        Assert.Equal(expected.PaymentDueDate, cycle.PaymentDueDate);
    }

    [Fact]
    public async Task Test_1_2_CreateDeferred_updates_cycle_total_and_card_balance()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var genResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = h.CreditCardSourceId },
            FinanceApiWireJson.Web);
        genResp.EnsureSuccessStatusCode();
        var cycleId = await FinanceApiWireJson.ReadBillingCycleIdFromGenerateResponseAsync(genResp);

        const long spend = 250;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "deferred",
                spend,
                h.CreditCardSourceId,
                h.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null,
                null,
                cycleId),
            FinanceApiWireJson.Web);
        createResp.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cycle = await db.FinBillingCycles.AsNoTracking().SingleAsync(c => c.Id == cycleId);
        var card = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == h.CreditCardSourceId);

        Assert.Equal(spend, cycle.TotalAmount);
        Assert.Equal(spend, card.Balance);
    }

    [Fact]
    public async Task Test_1_3_CloseBillingCycle_recomputes_total_from_deferred_transactions()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var genResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = h.CreditCardSourceId },
            FinanceApiWireJson.Web);
        genResp.EnsureSuccessStatusCode();
        var cycleId = await FinanceApiWireJson.ReadBillingCycleIdFromGenerateResponseAsync(genResp);

        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var amt in new[] { 80L, 120L })
        {
            var r = await client.PostAsJsonAsync(
                "api/v1/finance/transactions",
                new CreateTransactionWire(
                    h.SmoduleId,
                    "deferred",
                    amt,
                    h.CreditCardSourceId,
                    h.ExpenseCategoryId,
                    txnDate,
                    null,
                    null,
                    null,
                    null,
                    cycleId),
                FinanceApiWireJson.Web);
            r.EnsureSuccessStatusCode();
        }

        var closeResp = await client.PostAsync($"api/v1/finance/billing-cycles/{cycleId}/close", null);
        closeResp.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cycle = await db.FinBillingCycles.AsNoTracking().SingleAsync(c => c.Id == cycleId);
        Assert.Equal(BillingCycleStatus.Closed, cycle.Status);
        Assert.Equal(200m, cycle.TotalAmount);
    }

    [Fact]
    public async Task Test_1_4_PayBillingCycle_partial_then_full_updates_paid_and_status()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

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
                600,
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

        (await client.PostAsync($"api/v1/finance/billing-cycles/{cycleId}/close", null)).EnsureSuccessStatusCode();

        var payPartial = await client.PostAsJsonAsync(
            $"api/v1/finance/billing-cycles/{cycleId}/pay",
            new { paymentSourceId = h.BankSourceId, amount = 200L },
            FinanceApiWireJson.Web);
        payPartial.EnsureSuccessStatusCode();

        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cycle = await db.FinBillingCycles.AsNoTracking().SingleAsync(c => c.Id == cycleId);
            Assert.Equal(BillingCycleStatus.Closed, cycle.Status);
            Assert.Equal(200m, cycle.PaidAmount);
        }

        var payRest = await client.PostAsJsonAsync(
            $"api/v1/finance/billing-cycles/{cycleId}/pay",
            new { paymentSourceId = h.BankSourceId, amount = 400L },
            FinanceApiWireJson.Web);
        payRest.EnsureSuccessStatusCode();

        await using (var scope2 = _fixture.Services.CreateAsyncScope())
        {
            var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            var cycle = await db.FinBillingCycles.AsNoTracking().SingleAsync(c => c.Id == cycleId);
            Assert.Equal(BillingCycleStatus.Paid, cycle.Status);
            Assert.Equal(600m, cycle.PaidAmount);
        }
    }

    [Fact]
    public async Task Test_1_5_CloseMonth_with_open_cycle_throws_BusinessRuleException()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedCreditCardScenarioAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        var genResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = h.CreditCardSourceId },
            FinanceApiWireJson.Web);
        genResp.EnsureSuccessStatusCode();

        var (_, periodEnd, _, _) = BillingCycleDateMath.ExpectedForStatementDay(15, 25);

        var closeMonthResp = await client.PostAsJsonAsync(
            "api/v1/finance/monthly-periods/close",
            new { smoduleId = h.SmoduleId, year = periodEnd.Year, month = periodEnd.Month },
            FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, closeMonthResp.StatusCode);
        var msg = await FinanceApiWireJson.ReadFirstBusinessRuleMessageAsync(closeMonthResp);
        Assert.NotNull(msg);
        Assert.StartsWith("Còn ", msg, StringComparison.Ordinal);
        Assert.Contains(h.CreditCardName, msg, StringComparison.Ordinal);
    }

    /// <summary>
    /// Installment conversion-fee billing uses <see cref="ProcessConversionFeeCommand"/> (also invoked from
    /// <c>GenerateBillingCycle</c>). A second <c>generate</c> for the same statement window is rejected,
    /// so this harness calls MediatR directly to exercise fee posting against the open cycle.
    /// </summary>
    internal static async Task RunProcessConversionFeeAsync(IntegrationTestFixture fixture, Guid cycleId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var med = scope.ServiceProvider.GetRequiredService<IMediator>();
        await med.Send(new ProcessConversionFeeCommand(cycleId));
    }

    private static async Task<CreditCardHarness> RegisterUserSeedCreditCardScenarioAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var email = $"it-bc-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Billing Cycle User" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        var (accessToken, _, spaceId) = await AuthApiWire.ReadRegisterPayloadAsync(regResp);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var smodule = await db.SpaceModules
            .FirstAsync(m => m.SpaceId == spaceId && m.ModuleCode == ModuleCode.Finance && m.IsEnabled);

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

        var cardName = "Integration Visa";
        var creditCard = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = cardName,
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
            accessToken,
            cardName);
    }

    private sealed record CreditCardHarness(
        Guid SmoduleId,
        Guid CreditCardSourceId,
        Guid BankSourceId,
        Guid ExpenseCategoryId,
        string AccessToken,
        string CreditCardName);
}
