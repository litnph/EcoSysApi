using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserLoginAttempt"/>. Maps to <c>USER_LOGIN_ATTEMPTS</c> (append-only).</summary>
public sealed class UserLoginAttemptConfiguration : IEntityTypeConfiguration<UserLoginAttempt>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserLoginAttempt> builder)
    {
        builder.Property(x => x.AttemptedEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(64);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        // Brute-force detection scans by (email, time) and (ip, time).
        builder.HasIndex(x => new { x.AttemptedEmail, x.CreatedAt });
        builder.HasIndex(x => new { x.IpAddress, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
               .WithMany(u => u.LoginAttempts)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
