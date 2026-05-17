using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserSession"/>. Maps to <c>USER_SESSIONS</c>.</summary>
public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(255);
        builder.Property(x => x.DeviceType).HasMaxLength(32);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt });
        builder.HasIndex(x => x.ExpiresAt);

        builder.HasOne(x => x.User)
               .WithMany(u => u.Sessions)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
