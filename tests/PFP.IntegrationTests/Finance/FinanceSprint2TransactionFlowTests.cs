using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Entities;
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

        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);

        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var source = await db.FinSources.AsNoTracking().SingleAsync(s => s.Id == harness.SourceAId);
        Assert.Equal(1000m - amount, source.Balance);
    }
}
