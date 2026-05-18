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

[CollectionDefinition("FinanceSprint3Split", DisableParallelization = true)]
public sealed class FinanceSprint3SplitCollection;

/// <summary>Sprint 3 — Split flow (spec test group 4: create split, settle one participant).</summary>
[Collection("FinanceSprint3Split")]
public sealed class FinanceSprint3SplitFlowTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint3SplitFlowTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Spec group 4: <c>CreateSplit</c> (two <c>pending</c> rows, payer debited) + <c>SettleSplit</c> for one participant.</summary>
    [Fact]
    public async Task Test_4_1_CreateSplit_and_4_2_SettleOneParticipant()
    {
        using var client = _fixture.CreateClient();
        var harness = await RegisterUserSeedFinanceAndIncomeCategoryAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        const long total = 300;
        var splits = new[]
        {
            new SplitItemWire("Alice", null, 100),
            new SplitItemWire("Bob", "0909123456", 150),
        };

        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                harness.SmoduleId,
                "split",
                total,
                harness.SourceAId,
                harness.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null,
                splits),
            FinanceApiWireJson.Web);

        createResp.EnsureSuccessStatusCode();

        var txnId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(createResp);

        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sourceA = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
            Assert.Equal(1000m - total, sourceA.Balance);

            var rows = await db.FinTxnSplits.AsNoTracking()
                .Where(s => s.TransactionId == txnId)
                .OrderBy(s => s.PersonName)
                .ToListAsync();
            Assert.Equal(2, rows.Count);
            Assert.All(rows, r => Assert.Equal(SplitStatus.Pending, r.Status));
            Assert.Equal(100m, rows.Single(r => r.PersonName == "Alice").Amount);
        }

        var pendingResp = await client.GetAsync($"api/v1/finance/splits/pending?smodule_id={harness.SmoduleId}");
        pendingResp.EnsureSuccessStatusCode();

        var splitId = await FindSplitIdAsync(_fixture, txnId, "Alice");

        var settleResp = await client.PostAsJsonAsync(
            $"api/v1/finance/splits/{splitId}/settle",
            new { paymentSourceId = harness.SourceBId, amount = (decimal?)null },
            FinanceApiWireJson.Web);
        settleResp.EnsureSuccessStatusCode();

        await using (var scope2 = _fixture.Services.CreateAsyncScope())
        {
            var db = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
            var alice = await db.FinTxnSplits.AsNoTracking().SingleAsync(s => s.Id == splitId);
            Assert.Equal(SplitStatus.Settled, alice.Status);
            Assert.NotNull(alice.SettledTxnId);

            var sourceBafter = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceBId);
            Assert.Equal(500m + 100m, sourceBafter.Balance);
        }
    }

    private static async Task<Guid> FindSplitIdAsync(IntegrationTestFixture fixture, Guid txnId, string personName)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.FinTxnSplits.AsNoTracking()
            .Where(s => s.TransactionId == txnId && s.PersonName == personName)
            .Select(s => s.Id)
            .SingleAsync();
    }

    private static async Task<SplitHarness> RegisterUserSeedFinanceAndIncomeCategoryAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var email = $"it-split-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Split User" },
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
            Name = "Food",
            Code = "exp-" + Guid.NewGuid().ToString("N")[..16],
            Kind = CategoryKind.Expense,
            Depth = 0,
            SortOrder = 0,
            IsDefault = false,
            IsSystem = false,
        };
        var income = new FinCategory
        {
            SmoduleId = smodule.Id,
            Name = "Reimbursements",
            Code = "inc-" + Guid.NewGuid().ToString("N")[..16],
            Kind = CategoryKind.Income,
            Depth = 0,
            SortOrder = 1,
            IsDefault = false,
            IsSystem = false,
        };
        db.FinCategories.AddRange(expense, income);

        var sourceA = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = "Wallet A",
            Type = SourceType.BankAccount,
            Balance = 1000m,
            Currency = "VND",
            SortOrder = 0,
        };
        var sourceB = new FinSource
        {
            SmoduleId = smodule.Id,
            Name = "Wallet B",
            Type = SourceType.BankAccount,
            Balance = 500m,
            Currency = "VND",
            SortOrder = 1,
        };
        db.FinSources.AddRange(sourceA, sourceB);
        await db.SaveChangesAsync();

        return new SplitHarness(smodule.Id, sourceA.Id, sourceB.Id, expense.Id, accessToken);
    }

    private sealed record SplitHarness(
        Guid SmoduleId,
        Guid SourceAId,
        Guid SourceBId,
        Guid ExpenseCategoryId,
        string AccessToken);
}
