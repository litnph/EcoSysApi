using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;
using PFP.Infrastructure.Persistence;
using PFP.IntegrationTests.Auth;

namespace PFP.IntegrationTests.Finance;

internal static class FinanceTestHarness
{
    internal static async Task<FinanceHarness> SeedAndLoginAsync(
        WebApplicationFactory<Program> factory,
        HttpClient client,
        decimal sourceABalance = 1000m,
        decimal sourceBBalance = 500m)
    {
        const string password = "TestPass123!";
        var email = $"it-{Guid.NewGuid():N}@integration.test";
        Guid sourceAId;
        Guid sourceBId;
        Guid categoryId;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.Users.Add(new User
            {
                Email = email,
                FullName = "Integration User",
                PasswordHash = hasher.Hash(password),
                Role = UserRole.Member,
                IsActive = true,
            });

            var category = new FinCategory
            {
                Name = "Food",
                Code = "exp-" + Guid.NewGuid().ToString("N")[..16],
                Kind = CategoryKind.Expense,
                Depth = 0,
                SortOrder = 0,
            };
            db.FinCategories.Add(category);
            db.FinCategories.Add(new FinCategory
            {
                Name = "Salary",
                Code = "inc-" + Guid.NewGuid().ToString("N")[..16],
                Kind = CategoryKind.Income,
                Depth = 0,
                SortOrder = 0,
            });

            var sourceA = new FinSource
            {
                Name = "Wallet A",
                Type = SourceType.BankAccount,
                Balance = sourceABalance,
                Currency = "VND",
                SortOrder = 0,
            };
            var sourceB = new FinSource
            {
                Name = "Wallet B",
                Type = SourceType.BankAccount,
                Balance = sourceBBalance,
                Currency = "VND",
                SortOrder = 1,
            };
            db.FinSources.Add(sourceA);
            db.FinSources.Add(sourceB);
            await db.SaveChangesAsync();

            sourceAId = sourceA.Id;
            sourceBId = sourceB.Id;
            categoryId = category.Id;
        }

        var loginResp = await client.PostAsJsonAsync(
            "api/v1/auth/login",
            new { email, password },
            FinanceApiWireJson.Web);
        loginResp.EnsureSuccessStatusCode();
        var (accessToken, _) = await AuthApiWire.ReadTokensAsync(loginResp);

        return new FinanceHarness(sourceAId, sourceBId, categoryId, accessToken);
    }

    internal sealed record FinanceHarness(
        Guid SourceAId,
        Guid SourceBId,
        Guid ExpenseCategoryId,
        string AccessToken);
}
