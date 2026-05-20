using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Auth;
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

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_direct_transaction_updates_balance_history_and_audit()
    {
        using var client = _fixture.CreateClient();
        var harness = await RegisterUserAndSeedFinanceAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        const long amount = 100;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                harness.SmoduleId,
                "direct",
                amount,
                harness.SourceAId,
                harness.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null),
            FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);

        var txnId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(createResp);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var source = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
        Assert.Equal(1000m - amount, source.Balance);

        var history = await db.FinTransactionHistory.AsNoTracking()
            .Where(h => h.TransactionId == txnId)
            .SingleAsync();
        Assert.Equal(1, history.Version);
        Assert.Equal(HistoryChangeType.Created, history.ChangeType);

        var audit = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityId == txnId && a.Action == AuditAction.Created)
            .SingleAsync();
        Assert.Equal(nameof(FinTransaction), audit.EntityType);
    }

    [Fact]
    public async Task Delete_direct_transaction_soft_deletes_reverts_balance_and_writes_reversal()
    {
        using var client = _fixture.CreateClient();
        var harness = await RegisterUserAndSeedFinanceAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        const long amount = 50;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                harness.SmoduleId,
                "direct",
                amount,
                harness.SourceAId,
                harness.ExpenseCategoryId,
                txnDate,
                null,
                null,
                null),
            FinanceApiWireJson.Web);
        createResp.EnsureSuccessStatusCode();
        var txnId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(createResp);

        var deleteReq = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/finance/transactions/{txnId}")
        {
            Content = JsonContent.Create(new { reason = "integration test" }, options: FinanceApiWireJson.Web),
        };
        var deleteResp = await client.SendAsync(deleteReq);
        Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var source = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
        Assert.Equal(1000m, source.Balance);

        var original = await db.FinTransactions.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(t => t.Id == txnId);
        Assert.True(original.IsDeleted);

        var reversal = await db.FinTransactions.AsNoTracking()
            .SingleAsync(t => t.Type == TransactionType.Reversal && t.RefTxnId == txnId);
        Assert.Equal(original.Amount, reversal.Amount);

        await db.FinTransactionHistory.AsNoTracking()
            .Where(h => h.TransactionId == txnId && h.ChangeType == HistoryChangeType.Deleted)
            .SingleAsync();

        var audit = await db.AuditLogs.AsNoTracking()
            .Where(a => a.EntityId == txnId && a.Action == AuditAction.Deleted)
            .SingleAsync();
        Assert.Equal("fin_transactions", audit.EntityType);
    }

    [Fact]
    public async Task Create_transfer_creates_linked_pair_and_moves_balances()
    {
        using var client = _fixture.CreateClient();
        var harness = await RegisterUserAndSeedFinanceAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        const long amount = 200;
        var txnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var createResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                harness.SmoduleId,
                "transfer",
                amount,
                harness.SourceAId,
                null,
                txnDate,
                null,
                null,
                harness.SourceBId),
            FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var outboundId = await FinanceApiWireJson.ReadTransactionIdFromCreateResponseAsync(createResp);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var transfers = await db.FinTransactions.AsNoTracking()
            .Where(t => t.SmoduleId == harness.SmoduleId && t.Type == TransactionType.Transfer && t.TxnDate == txnDate)
            .ToListAsync();

        Assert.Equal(2, transfers.Count);
        var outbound = transfers.Single(t => t.SourceId == harness.SourceAId);
        var inbound = transfers.Single(t => t.SourceId == harness.SourceBId);
        Assert.Equal(inbound.Id, outbound.RefTxnId);
        Assert.Equal(outbound.Id, inbound.RefTxnId);
        Assert.Equal(-amount, outbound.Amount);
        Assert.Equal(amount, inbound.Amount);

        var srcA = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
        var srcB = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceBId);
        Assert.Equal(1000m - amount, srcA.Balance);
        Assert.Equal(500m + amount, srcB.Balance);

        Assert.Equal(outboundId, outbound.Id);
    }

    private static async Task<FinanceHarness> RegisterUserAndSeedFinanceAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client)
    {
        var email = $"it-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Integration User" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        var (accessToken, _, spaceId) = await AuthApiWire.ReadRegisterPayloadAsync(regResp);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var smodule = await db.SpaceModules
            .FirstAsync(m => m.SpaceId == spaceId && m.ModuleCode == ModuleCode.Finance && m.IsEnabled);

        var category = new FinCategory
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
        db.FinCategories.Add(category);

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
        db.FinSources.Add(sourceA);
        db.FinSources.Add(sourceB);

        await db.SaveChangesAsync();

        return new FinanceHarness(
            smodule.Id,
            sourceA.Id,
            sourceB.Id,
            category.Id,
            accessToken);
    }

    private sealed record FinanceHarness(
        Guid SmoduleId,
        Guid SourceAId,
        Guid SourceBId,
        Guid ExpenseCategoryId,
        string AccessToken);
}
