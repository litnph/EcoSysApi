using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserProfile"/>. Maps to <c>USER_PROFILES</c> (1-1 with <see cref="User"/>).</summary>
public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.Property(x => x.LanguageCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Timezone).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DateFormat).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Theme).HasMaxLength(16).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(255);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.User)
               .WithOne(u => u.Profile!)
               .HasForeignKey<UserProfile>(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Language)
               .WithMany()
               .HasForeignKey(x => x.LanguageCode)
               .HasPrincipalKey(l => l.Code)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
