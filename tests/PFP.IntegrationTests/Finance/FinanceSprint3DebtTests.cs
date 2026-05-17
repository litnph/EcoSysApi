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
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

/// <summary>Sprint 3 Task 7 — Debt / loan flows (test group 3).</summary>
[Collection("FinanceSprint3")]
public sealed class FinanceSprint3DebtTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint3DebtTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Test_3_1_debt_borrow_creates_record_and_increases_balance()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedWalletAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        const decimal amt = 1_000m;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var r = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "debtBorrow",
                amt,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                "Lender A",
                null,
                null,
                null),
            FinanceApiWireJson.Web);
        r.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.SmoduleId == h.SmoduleId);
        var wallet = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == h.WalletId);

        Assert.Equal(DebtDirection.Borrowed, debt.Direction);
        Assert.Equal(amt, debt.RemainingAmount);
        Assert.Equal(10_000m + amt, wallet.Balance);
    }

    [Fact]
    public async Task Test_3_2_debt_repay_partial_reduces_remaining_stays_active()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedWalletAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        await PostDebtBorrowAsync(client, h, 1_000m, "Lender P");
        var debtId = await ReadSingleDebtRecordIdAsync(_fixture, h.SmoduleId);

        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var r = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "debtRepay",
                400m,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                debtId,
                null),
            FinanceApiWireJson.Web);
        r.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.Id == debtId);
        Assert.Equal(600m, debt.RemainingAmount);
        Assert.Equal(DebtStatus.Active, debt.Status);
    }

    [Fact]
    public async Task Test_3_3_debt_repay_full_completes_and_zeroes_remaining()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedWalletAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        await PostDebtBorrowAsync(client, h, 500m, "Lender F");
        var debtId = await ReadSingleDebtRecordIdAsync(_fixture, h.SmoduleId);

        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var r = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "debtRepay",
                500m,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                debtId,
                null),
            FinanceApiWireJson.Web);
        r.EnsureSuccessStatusCode();

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.Id == debtId);
        Assert.Equal(DebtStatus.Completed, debt.Status);
        Assert.Equal(0m, debt.RemainingAmount);
    }

    [Fact]
    public async Task Test_3_4_loan_give_and_loan_collect_reverse_flow()
    {
        using var client = _fixture.CreateClient();
        var h = await RegisterUserSeedWalletAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);

        const decimal original = 800m;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var giveResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "loanGive",
                original,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                "Borrower B",
                null,
                null,
                null),
            FinanceApiWireJson.Web);
        giveResp.EnsureSuccessStatusCode();

        Guid debtId;
        await using (var scope0 = _fixture.Services.CreateAsyncScope())
        {
            var db = scope0.ServiceProvider.GetRequiredService<AppDbContext>();
            var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.SmoduleId == h.SmoduleId);
            debtId = debt.Id;
            Assert.Equal(DebtDirection.Lent, debt.Direction);
            Assert.Equal(original, debt.RemainingAmount);
            var wallet = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == h.WalletId);
            Assert.Equal(10_000m - original, wallet.Balance);
        }

        var partial = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "loanCollect",
                300m,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                debtId,
                null),
            FinanceApiWireJson.Web);
        partial.EnsureSuccessStatusCode();

        await using (var scope1 = _fixture.Services.CreateAsyncScope())
        {
            var db = scope1.ServiceProvider.GetRequiredService<AppDbContext>();
            var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.Id == debtId);
            Assert.Equal(500m, debt.RemainingAmount);
            Assert.Equal(DebtStatus.Active, debt.Status);
        }

        var full = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "loanCollect",
                500m,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                debtId,
                null),
            FinanceApiWireJson.Web);
        full.EnsureSuccessStatusCode();

        await using (var scope2 = _fixture.Services.CreateAsyncScope())
        {
            var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            var debt = await db.FinDebtRecords.AsNoTracking().SingleAsync(d => d.Id == debtId);
            Assert.Equal(DebtStatus.Completed, debt.Status);
            Assert.Equal(0m, debt.RemainingAmount);
            var wallet = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == h.WalletId);
            Assert.Equal(10_000m, wallet.Balance);
        }
    }

    private static async Task PostDebtBorrowAsync(HttpClient client, DebtHarness h, decimal amount, string lender)
    {
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var r = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                h.SmoduleId,
                "debtBorrow",
                amount,
                h.WalletId,
                null,
                txnDate,
                null,
                null,
                null,
                null,
                null,
                lender,
                null,
                null,
                null),
            FinanceApiWireJson.Web);
        r.EnsureSuccessStatusCode();
    }

    private static async Task<Guid> ReadSingleDebtRecordIdAsync(IntegrationTestFixture fixture, Guid smoduleId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.FinDebtRecords.AsNoTracking()
            .Where(d => d.SmoduleId == smoduleId)
            .Select(d => d.Id)
            .SingleAsync();
    }

    private static async Task<DebtHarness> RegisterUserSeedWalletAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var email = $"it-debt-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Debt User" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        await using var regStream = await regResp.Content.ReadAsStreamAsync();
        using var regDoc = await JsonDocument.ParseAsync(regStream);
        var root = regDoc.RootElement;
        var spaceId = root.GetProperty("personalSpaceId").GetGuid();
        var accessToken = root.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing accessToken.");

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

        var wallet = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = "Cash wallet",
            Type = SourceType.Cash,
            Balance = 10_000m,
            Currency = "VND",
            SortOrder = 0,
        };
        db.FinSources.Add(wallet);
        await db.SaveChangesAsync();

        return new DebtHarness(smodule.Id, wallet.Id, accessToken);
    }

    private sealed record DebtHarness(Guid SmoduleId, Guid WalletId, string AccessToken);
}
