using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Finance;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[Collection("FinanceSprint4")]
public sealed class FinanceSprint4SpaceMemberInvitationTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public FinanceSprint4SpaceMemberInvitationTests(IntegrationTestFixture fixture) => _fixture = fixture;

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
    public async Task Invite_org_member_propagates_inherited_roles_to_existing_child_space()
    {
        using var client = _fixture.CreateClient();

        var emailA = $"it-a-{Guid.NewGuid():N}@invite.test";
        var regA = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email = emailA, password = "TestPass123", fullName = "Owner A" },
            FinanceApiWireJson.Web);
        regA.EnsureSuccessStatusCode();
        JsonElement rootA;
        string tokenA;
        Guid orgId;
        Guid rootSpaceId;
        {
            await using var s = await regA.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            rootA = doc.RootElement.Clone();
            tokenA = rootA.GetProperty("accessToken").GetString()
                     ?? throw new InvalidOperationException("Missing access token.");
            orgId = rootA.GetProperty("organizationId").GetGuid();
            rootSpaceId = rootA.GetProperty("personalSpaceId").GetGuid();
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var childResp = await client.PostAsJsonAsync(
            "api/v1/spaces",
            new
            {
                orgId,
                name = "Child",
                type = "family",
                parentId = rootSpaceId,
                sortOrder = 0,
            },
            FinanceApiWireJson.Web);
        childResp.EnsureSuccessStatusCode();
        Guid childId;
        {
            await using var s = await childResp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            childId = doc.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        }

        var emailB = $"it-b-{Guid.NewGuid():N}@invite.test";
        Guid userBId;
        {
            var regB = await client.PostAsJsonAsync(
                "api/v1/auth/register",
                new { email = emailB, password = "TestPass123", fullName = "Member B" },
                FinanceApiWireJson.Web);
            regB.EnsureSuccessStatusCode();
            await using var s = await regB.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(s);
            userBId = doc.RootElement.GetProperty("userId").GetGuid();
        }

        await using (var seedScope = _fixture.Services.CreateAsyncScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OrgMembers.Add(
                new OrgMember
                {
                    OrgId = orgId,
                    UserId = userBId,
                    Role = OrgRole.Member,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                });
            await db.SaveChangesAsync();
        }

        var inviteResp = await client.PostAsJsonAsync(
            $"api/v1/spaces/{rootSpaceId:D}/members",
            new { userId = userBId, role = "editor" },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.OK, inviteResp.StatusCode);

        await using (var verify = _fixture.Services.CreateAsyncScope())
        {
            var db = verify.ServiceProvider.GetRequiredService<AppDbContext>();

            var direct = await db.SpaceMembers.SingleAsync(m =>
                m.SpaceId == rootSpaceId && m.UserId == userBId && m.LeftAt == null);
            Assert.False(direct.Inherited);
            Assert.Equal(SpaceRole.Editor, direct.Role);

            var inherited = await db.SpaceMembers.SingleAsync(m =>
                m.SpaceId == childId && m.UserId == userBId && m.LeftAt == null);
            Assert.True(inherited.Inherited);
            Assert.Equal(rootSpaceId, inherited.InheritedFromSpaceId);
            Assert.Equal(SpaceRole.Editor, inherited.Role);
        }
    }
}
