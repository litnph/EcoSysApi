using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserPasswordReset"/>. Maps to <c>USER_PASSWORD_RESETS</c>.</summary>
public sealed class UserPasswordResetConfiguration : IEntityTypeConfiguration<UserPasswordReset>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserPasswordReset> builder)
    {
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RequestIpAddress).HasMaxLength(64);
        builder.Property(x => x.UsedIpAddress).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne(x => x.User)
               .WithMany(u => u.PasswordResets)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
