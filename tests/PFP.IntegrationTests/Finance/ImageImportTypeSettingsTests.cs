using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Finance;

[Collection("FinanceSprint2")]
public sealed class ImageImportTypeSettingsTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public ImageImportTypeSettingsTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Workspace_settings_round_trip_independently_from_user_profile()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(
            _fixture,
            client,
            role: UserRole.Admin);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var updateResponse = await client.PutAsJsonAsync(
            "api/v1/settings/image-import-types",
            new
            {
                settings = new object[]
                {
                    new { type = "statement", displayName = "Sao kê Visa", sourceId = harness.SourceAId },
                    new { type = "bank_transaction_list", displayName = "Danh sách ngân hàng", sourceId = (Guid?)null },
                    new { type = "tp", displayName = "TP", sourceId = harness.SourceBId },
                },
            });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var getResponse = await client.GetAsync("api/v1/settings/image-import-types");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        await using (var stream = await getResponse.Content.ReadAsStreamAsync())
        using (var json = await JsonDocument.ParseAsync(stream))
        {
            var settings = json.RootElement.GetProperty("data").EnumerateArray().ToArray();
            Assert.Equal(3, settings.Length);
            Assert.Contains(settings, setting =>
                setting.GetProperty("type").GetString() == "statement"
                && setting.GetProperty("displayName").GetString() == "Sao kê Visa"
                && setting.GetProperty("sourceId").GetGuid() == harness.SourceAId);
            Assert.Contains(settings, setting =>
                setting.GetProperty("type").GetString() == "bank_transaction_list"
                && setting.GetProperty("sourceId").ValueKind == JsonValueKind.Null);
            Assert.Contains(settings, setting =>
                setting.GetProperty("type").GetString() == "tp"
                && setting.GetProperty("sourceId").GetGuid() == harness.SourceBId);
        }

        var profileResponse = await client.GetAsync("api/v1/user/profile");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        await using var profileStream = await profileResponse.Content.ReadAsStreamAsync();
        using var profileJson = await JsonDocument.ParseAsync(profileStream);
        var profile = profileJson.RootElement.GetProperty("data").GetProperty("profile");
        Assert.False(profile.TryGetProperty("imageImportTypeSettings", out _));
    }

    [Fact]
    public async Task Members_can_read_but_cannot_update_workspace_settings()
    {
        using var client = _fixture.CreateClient();
        var harness = await FinanceTestHarness.SeedAndLoginAsync(_fixture, client);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", harness.AccessToken);

        var getResponse = await client.GetAsync("api/v1/settings/image-import-types");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync(
            "api/v1/settings/image-import-types",
            new
            {
                settings = new object[]
                {
                    new { type = "statement", displayName = "Sao kê", sourceId = (Guid?)null },
                    new { type = "bank_transaction_list", displayName = "Danh sách giao dịch", sourceId = (Guid?)null },
                    new { type = "tp", displayName = "TP", sourceId = (Guid?)null },
                },
            });

        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }
}
