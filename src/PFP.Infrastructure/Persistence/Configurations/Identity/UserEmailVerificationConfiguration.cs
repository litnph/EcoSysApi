using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserEmailVerification"/>. Maps to <c>USER_EMAIL_VERIFICATIONS</c>.</summary>
public sealed class UserEmailVerificationConfiguration : IEntityTypeConfiguration<UserEmailVerification>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserEmailVerification> builder)
    {
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.NewEmail).HasMaxLength(320);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Type });
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne(x => x.User)
               .WithMany(u => u.EmailVerifications)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
