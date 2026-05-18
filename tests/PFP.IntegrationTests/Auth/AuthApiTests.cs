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

namespace PFP.IntegrationTests.Auth;

[CollectionDefinition("AuthApi", DisableParallelization = true)]
public sealed class AuthApiCollection;

/// <summary>Covers <c>api/v1/auth</c> register / login / refresh / logout and related error paths.</summary>
[Collection("AuthApi")]
public sealed class AuthApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public AuthApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

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
    public async Task Register_login_refresh_logout_revokes_refresh_token()
    {
        using var client = _fixture.CreateClient();
        const string password = "TestPass123";
        var email = $"auth-it-{Guid.NewGuid():N}@integration.test";

        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password, fullName = "Auth API Test" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();
        await using (var regStream = await regResp.Content.ReadAsStreamAsync())
        {
            using var regDoc = await JsonDocument.ParseAsync(regStream);
            var root = regDoc.RootElement;
            Assert.False(string.IsNullOrEmpty(root.GetProperty("accessToken").GetString()));
        }

        var loginResp = await client.PostAsJsonAsync(
            "api/v1/auth/login",
            new { email, password },
            FinanceApiWireJson.Web);
        loginResp.EnsureSuccessStatusCode();
        string accessFromLogin;
        string refreshFromLogin;
        await using (var loginStream = await loginResp.Content.ReadAsStreamAsync())
        {
            using var loginDoc = await JsonDocument.ParseAsync(loginStream);
            var root = loginDoc.RootElement;
            accessFromLogin = root.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Missing accessToken.");
            refreshFromLogin = root.GetProperty("refreshToken").GetString()
                ?? throw new InvalidOperationException("Missing refreshToken.");
        }

        var refreshResp = await client.PostAsJsonAsync(
            "api/v1/auth/refresh",
            new { refreshToken = refreshFromLogin },
            FinanceApiWireJson.Web);
        refreshResp.EnsureSuccessStatusCode();
        string access2;
        string refresh2;
        await using (var refreshStream = await refreshResp.Content.ReadAsStreamAsync())
        {
            using var refreshDoc = await JsonDocument.ParseAsync(refreshStream);
            var root = refreshDoc.RootElement;
            access2 = root.GetProperty("accessToken").GetString()
                ?? throw new InvalidOperationException("Missing accessToken.");
            refresh2 = root.GetProperty("refreshToken").GetString()
                ?? throw new InvalidOperationException("Missing refreshToken.");
        }

        Assert.NotEqual(refreshFromLogin, refresh2);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access2);
        var logoutResp = await client.PostAsync("api/v1/auth/logout", null);
        logoutResp.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = null;

        var refreshAfterLogout = await client.PostAsJsonAsync(
            "api/v1/auth/refresh",
            new { refreshToken = refresh2 },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        using var client = _fixture.CreateClient();
        const string password = "TestPass123";
        var email = $"auth-it-{Guid.NewGuid():N}@integration.test";

        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password, fullName = "Auth API Test" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        var badLogin = await client.PostAsJsonAsync(
            "api/v1/auth/login",
            new { email, password = "WrongPass123" },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.Unauthorized, badLogin.StatusCode);
    }

    [Fact]
    public async Task Forgot_password_returns_ok_for_registered_email()
    {
        using var client = _fixture.CreateClient();
        const string password = "TestPass123";
        var email = $"auth-it-{Guid.NewGuid():N}@integration.test";

        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password, fullName = "Auth API Test" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();

        var forgotResp = await client.PostAsJsonAsync(
            "api/v1/auth/forgot-password",
            new { email },
            FinanceApiWireJson.Web);
        forgotResp.EnsureSuccessStatusCode();
        await using var stream = await forgotResp.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.True(doc.RootElement.TryGetProperty("message", out var msg));
        Assert.False(string.IsNullOrWhiteSpace(msg.GetString()));
    }

    [Fact]
    public async Task Register_succeeds_when_user_agent_exceeds_512_chars()
    {
        using var client = _fixture.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", new string('x', 900));

        const string password = "TestPass123";
        var email = $"auth-it-{Guid.NewGuid():N}@integration.test";

        var regResp = await client.PostAsJsonAsync(
            "api/v1/auth/register",
            new { email, password, fullName = "Long UA Client" },
            FinanceApiWireJson.Web);
        regResp.EnsureSuccessStatusCode();
        await using var stream = await regResp.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        Assert.False(string.IsNullOrEmpty(doc.RootElement.GetProperty("accessToken").GetString()));
    }
}
