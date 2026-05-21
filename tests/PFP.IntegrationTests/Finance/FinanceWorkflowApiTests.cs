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

[CollectionDefinition("FinanceWorkflow", DisableParallelization = true)]
public sealed class FinanceWorkflowCollection;

[Collection("FinanceWorkflow")]
public sealed class FinanceWorkflowApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceWorkflowApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Finance_workflow_all_major_endpoints_succeed()
    {
        using var client = _fixture.CreateClient();
        var h = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", h.AccessToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Sources
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/sources")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"api/v1/finance/sources/{h.SourceAId}")).StatusCode);

        // Categories
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/categories?kind=expense")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/categories/flat?kind=expense")).StatusCode);

        // Direct transaction (fixed retry+transaction bug)
        var directResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire("direct", 50, h.SourceAId, h.ExpenseCategoryId, today, null, null, null),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, directResp.StatusCode);

        // Income
        Guid incomeCatId;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            incomeCatId = await db.FinCategories
                .Where(c => c.Kind == CategoryKind.Income)
                .Select(c => c.Id)
                .FirstAsync();
            db.FinCategories.Add(new FinCategory
            {
                Name = "IT Income",
                Code = "it-inc-" + Guid.NewGuid().ToString("N")[..8],
                Kind = CategoryKind.Income,
                Depth = 0,
                SortOrder = 99,
            });
            await db.SaveChangesAsync();
            incomeCatId = await db.FinCategories.OrderByDescending(c => c.CreatedAt).Select(c => c.Id).FirstAsync();
        }

        var incomeResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire("income", 200, h.SourceAId, incomeCatId, today, null, null, null),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, incomeResp.StatusCode);

        // Transfer
        var transferResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire("transfer", 100, h.SourceAId, null, today, null, null, h.SourceBId),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, transferResp.StatusCode);

        // List transactions
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/transactions?page=1&page_size=10")).StatusCode);

        // Credit card + billing
        var ccResp = await client.PostAsJsonAsync(
            "api/v1/finance/sources",
            new
            {
                name = "IT Visa",
                type = "creditCard",
                currency = "VND",
                creditLimit = 50_000_000L,
                statementDay = 10,
                paymentDueDay = 25,
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, ccResp.StatusCode);

        await using var scope2 = _fixture.Services.CreateAsyncScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        var ccId = await db2.FinSources.Where(s => s.Name == "IT Visa").Select(s => s.Id).FirstAsync();

        var genCycleResp = await client.PostAsJsonAsync(
            "api/v1/finance/billing-cycles/generate",
            new { sourceId = ccId },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, genCycleResp.StatusCode);

        var cycleId = await db2.FinBillingCycles.Where(c => c.SourceId == ccId).Select(c => c.Id).FirstAsync();

        var deferredResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "deferred", 500, ccId, h.ExpenseCategoryId, today, null, null, null, null, cycleId),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, deferredResp.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/billing-cycles")).StatusCode);

        var closeCycleResp = await client.PostAsync($"api/v1/finance/billing-cycles/{cycleId}/close", null);
        Assert.Equal(HttpStatusCode.OK, closeCycleResp.StatusCode);

        // Debt borrow
        var borrowResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "debtBorrow", 1000, h.SourceAId, null, today, null, null, null,
                null, null, "Test Person", null, null, today.AddMonths(1)),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, borrowResp.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/debt-records")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/debt-records/summary")).StatusCode);

        // Split
        var splitResp = await client.PostAsJsonAsync(
            "api/v1/finance/transactions",
            new CreateTransactionWire(
                "split", 300, h.SourceAId, h.ExpenseCategoryId, today, null, null, null,
                new[] { new SplitItemWire("Alice", null, 150), new SplitItemWire("Bob", null, 150) }),
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, splitResp.StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/splits/pending")).StatusCode);

        // Monthly periods / reports
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/monthly-periods")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/monthly-periods/current")).StatusCode);

        // Tags
        var tagResp = await client.PostAsJsonAsync(
            "api/v1/finance/tags",
            new { name = "it-tag-" + Guid.NewGuid().ToString("N")[..6], color = "#FF0000" },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, tagResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/tags")).StatusCode);

        // Savings
        var savingResp = await client.PostAsJsonAsync(
            "api/v1/finance/savings",
            new
            {
                sourceId = h.SourceBId,
                name = "IT Saving",
                targetAmount = 1_000_000L,
                interestRate = 5.5m,
                startDate = today,
                type = "flexible",
                status = "active",
            },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, savingResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/savings")).StatusCode);

        // Investments
        var invResp = await client.PostAsJsonAsync(
            "api/v1/finance/investments",
            new { name = "IT Stock", type = "stock", currency = "VND" },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, invResp.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/finance/investments")).StatusCode);

        // User profile
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/user/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("api/v1/user/profile")).StatusCode);
    }
}
