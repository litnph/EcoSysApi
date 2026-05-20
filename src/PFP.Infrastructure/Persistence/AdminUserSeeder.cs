using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>Creates a default admin user when seeding is enabled and no users exist.</summary>
public static class AdminUserSeeder
{
    public static async Task EnsureAsync(
        AppDbContext db,
        IConfiguration configuration,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken).ConfigureAwait(false))
            return;

        var email = configuration["Seed:AdminEmail"] ?? "admin@pfp.local";
        var password = configuration["Seed:AdminPassword"] ?? "ChangeMe123!";
        var fullName = configuration["Seed:AdminFullName"] ?? "Admin";

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHasher.Hash(password),
            Role = UserRole.Admin,
            IsActive = true,
        };

        db.Users.Add(user);
        db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            LanguageCode = "vi",
            Timezone = "Asia/Ho_Chi_Minh",
            DateFormat = "dd/MM/yyyy",
            Theme = "system",
        });

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
