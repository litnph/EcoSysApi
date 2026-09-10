using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[Collection("ApprovedBusinessRules")]
public sealed class TransactionClassificationRuleApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    public TransactionClassificationRuleApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Rules_are_user_owned_validate_duplicates_and_classify_import_without_overwriting_explicit_values()
    {
        using var client = _fixture.CreateClient();
        var first = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client, sourceABalance: 10_000m);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", first.AccessToken);

        Guid tagId;
        Guid overrideCategoryId;
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tag = new Tag { Name = $"Delivery-{Guid.NewGuid():N}", Color = "#336699" };
            var category = new FinCategory
            {
                Name = "Manual override", Code = $"manual-{Guid.NewGuid():N}"[..32],
                Kind = CategoryKind.Expense, Depth = 0, SortOrder = 10,
            };
            db.Tags.Add(tag);
            db.FinCategories.Add(category);
            await db.SaveChangesAsync();
            tagId = tag.Id;
            overrideCategoryId = category.Id;
        }

        var create = await client.PostAsJsonAsync(
            "api/v1/finance/transaction-classification-rules",
            new { keyword = " BE ", categoryId = first.ExpenseCategoryId, tagId, isActive = true },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var ruleId = (await ReadJsonAsync(create)).RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var duplicate = await client.PostAsJsonAsync(
            "api/v1/finance/transaction-classification-rules",
            new { keyword = "be", categoryId = first.ExpenseCategoryId, tagId = (Guid?)null, isActive = true },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicate.StatusCode);

        var match = await client.PostAsJsonAsync(
            "api/v1/finance/transaction-classification-rules/match",
            new { items = new[] { new { key = "one", content = "Thanh toan BE.COM" }, new { key = "two", content = "BEAUTY" } } },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, match.StatusCode);
        var matches = (await ReadJsonAsync(match)).RootElement.GetProperty("data");
        Assert.Equal(first.ExpenseCategoryId, matches[0].GetProperty("categoryId").GetGuid());
        Assert.Equal(JsonValueKind.Null, matches[1].GetProperty("ruleId").ValueKind);

        var automaticKey = Guid.NewGuid();
        var automatic = ImportItem(automaticKey, first.SourceAId, null, null, "BE - Delivery");
        var preview = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/preview", new { items = new[] { automatic } }, FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.False(await TransactionExistsAsync(automaticKey));

        var commit = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit", new { items = new[] { automatic } }, FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, commit.StatusCode);
        var automaticTransactionId = await TransactionIdAsync(automaticKey);
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var transaction = await db.FinTransactions.AsNoTracking().SingleAsync(x => x.Id == automaticTransactionId);
            Assert.Equal(first.ExpenseCategoryId, transaction.CategoryId);
            Assert.True(await db.EntityTags.AnyAsync(x => x.EntityId == automaticTransactionId && x.TagId == tagId));
        }

        var overrideKey = Guid.NewGuid();
        var explicitOverride = ImportItem(overrideKey, first.SourceAId, overrideCategoryId, Array.Empty<Guid>(), "BE");
        var overrideCommit = await client.PostAsJsonAsync(
            "api/v1/finance/transactions/import/commit", new { items = new[] { explicitOverride } }, FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, overrideCommit.StatusCode);
        var overrideTransactionId = await TransactionIdAsync(overrideKey);
        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.Equal(overrideCategoryId, (await db.FinTransactions.AsNoTracking().SingleAsync(x => x.Id == overrideTransactionId)).CategoryId);
            Assert.False(await db.EntityTags.AnyAsync(x => x.EntityId == overrideTransactionId));
        }

        var second = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", second.AccessToken);
        var otherList = await client.GetAsync("api/v1/finance/transaction-classification-rules");
        Assert.Equal(0, (await ReadJsonAsync(otherList)).RootElement.GetProperty("data").GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.DeleteAsync($"api/v1/finance/transaction-classification-rules/{ruleId}")).StatusCode);
    }

    private static object ImportItem(Guid key, Guid sourceId, Guid? categoryId, Guid[]? tagIds, string description) => new
    {
        type = "direct", amount = 100L, sourceId, categoryId,
        txnDate = DateOnly.FromDateTime(DateTime.UtcNow), description, clientRequestId = key, tagIds,
    };

    private async Task<bool> TransactionExistsAsync(Guid key)
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().FinTransactions.AnyAsync(x => x.ClientRequestId == key);
    }

    private async Task<Guid> TransactionIdAsync(Guid key)
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().FinTransactions
            .Where(x => x.ClientRequestId == key).Select(x => x.Id).SingleAsync();
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync());
}
