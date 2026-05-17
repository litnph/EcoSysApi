using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Finance;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[CollectionDefinition("FinanceSprint4", DisableParallelization = true)]
public sealed class FinanceSprint4Collection;

[Collection("FinanceSprint4")]
public sealed class FinanceSprint4SpaceTreeTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint4SpaceTreeTests(IntegrationTestFixture fixture) => _fixture = fixture;

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
    public async Task Create_move_and_tree_reflect_materialised_paths_and_inherited_membership()
    {
        using var client = _fixture.CreateClient();
        var email = $"it-space-{Guid.NewGuid():N}@integration.test";
        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password = "TestPass123", fullName = "Space Tree User" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        await using var regStream = await regResp.Content.ReadAsStreamAsync();
        using var regDoc = await JsonDocument.ParseAsync(regStream);
        var regRoot = regDoc.RootElement;
        var accessToken = regRoot.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing accessToken.");
        var organizationId = regRoot.GetProperty("organizationId").GetGuid();
        var rootSpaceId = regRoot.GetProperty("personalSpaceId").GetGuid();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await using var pathScope = _fixture.Services.CreateAsyncScope();
        var pathDb = pathScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var registrationRootPath = await pathDb.Spaces.AsNoTracking()
            .Where(s => s.Id == rootSpaceId)
            .Select(s => s.Path)
            .SingleAsync();

        var createResp = await client.PostAsJsonAsync(
            "api/v1/spaces",
            new
            {
                orgId = organizationId,
                name = "Child space",
                type = "family",
                parentId = rootSpaceId,
                description = (string?)null,
                sortOrder = 10,
            },
            FinanceApiWireJson.Web);

        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);

        await using var createDocStream = await createResp.Content.ReadAsStreamAsync();
        using var createDoc = await JsonDocument.ParseAsync(createDocStream);
        var created = createDoc.RootElement.GetProperty("data");
        var childId = created.GetProperty("id").GetGuid();
        var childPathExpected = $"{registrationRootPath}/{childId}";
        Assert.Equal(childPathExpected, created.GetProperty("path").GetString());

        await using var scopeVerify = _fixture.Services.CreateAsyncScope();
        var db = scopeVerify.ServiceProvider.GetRequiredService<AppDbContext>();

        var inheritedCount = await db.SpaceMembers.CountAsync(
            m => m.SpaceId == childId && m.Inherited && m.LeftAt == null);
        Assert.Equal(1, inheritedCount);

        var treeResp = await client.GetAsync(
            $"api/v1/spaces/tree?org_id={organizationId:D}");
        treeResp.EnsureSuccessStatusCode();

        await using var treeDocStream = await treeResp.Content.ReadAsStreamAsync();
        using var treeDoc = await JsonDocument.ParseAsync(treeDocStream);
        var roots = treeDoc.RootElement.GetProperty("data").GetProperty("roots");
        Assert.True(TryFindNodeById(roots, rootSpaceId, out var rootFound));
        Assert.True(rootFound.TryGetProperty("children", out var childrenArr));
        Assert.True(TryFindNodeById(childrenArr, childId, out var childInTree));
        Assert.True(childInTree.TryGetProperty("financeModuleEnabled", out var fm));
        Assert.False(fm.GetBoolean());

        var moveResp = await client.PostAsJsonAsync(
            $"api/v1/spaces/{childId}/move",
            new { newParentId = (Guid?)null },
            FinanceApiWireJson.Web);
        moveResp.EnsureSuccessStatusCode();

        var movedExpectedPath = $"/{organizationId}/{childId}";
        var childRow = await db.Spaces.AsNoTracking().SingleAsync(s => s.Id == childId);
        Assert.Equal(movedExpectedPath, childRow.Path);
        Assert.Equal(0, childRow.Depth);
        Assert.Null(childRow.ParentId);
    }

    private static bool TryFindNodeById(JsonElement array, Guid id, out JsonElement node)
    {
        foreach (var el in array.EnumerateArray())
        {
            if (el.TryGetProperty("id", out var nid) && nid.GetGuid() == id)
            {
                node = el;
                return true;
            }

            if (!el.TryGetProperty("children", out var nested) || nested.ValueKind != JsonValueKind.Array)
                continue;

            if (TryFindNodeById(nested, id, out node))
                return true;
        }

        node = default;
        return false;
    }
}
