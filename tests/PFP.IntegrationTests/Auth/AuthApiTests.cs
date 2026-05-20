using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Finance;
using PFP.IntegrationTests.Support;
using Xunit;

namespace PFP.IntegrationTests.Auth;

[CollectionDefinition("AuthApi", DisableParallelization = true)]
public sealed class AuthApiCollection;

[Collection("AuthApi")]
public sealed class AuthApiTests : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;

    public AuthApiTests(IntegrationTestFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Login_refresh_logout_revokes_refresh_token()
    {
        using var client = _fixture.CreateClient();
        const string password = "TestPass123!";
        var email = $"auth-it-{Guid.NewGuid():N}@integration.test";

        await using (var scope = _fixture.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            db.Users.Add(new User
            {
                Email = email,
                FullName = "Auth API Test",
                PasswordHash = hasher.Hash(password),
                Role = UserRole.Member,
                IsActive = true,
            });
            await db.SaveChangesAsync();
        }

        var loginResp = await client.PostAsJsonAsync(
            "api/v1/auth/login",
            new { email, password },
            FinanceApiWireJson.Web);
        loginResp.EnsureSuccessStatusCode();
        var (access, refresh) = await AuthApiWire.ReadTokensAsync(loginResp);

        var refreshResp = await client.PostAsJsonAsync(
            "api/v1/auth/refresh",
            new { refreshToken = refresh },
            FinanceApiWireJson.Web);
        refreshResp.EnsureSuccessStatusCode();
        var (_, refresh2) = await AuthApiWire.ReadTokensAsync(refreshResp);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access);

        var logoutResp = await client.PostAsync("api/v1/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResp.StatusCode);

        var reuseResp = await client.PostAsJsonAsync(
            "api/v1/auth/refresh",
            new { refreshToken = refresh2 },
            FinanceApiWireJson.Web);
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResp.StatusCode);
    }
}
