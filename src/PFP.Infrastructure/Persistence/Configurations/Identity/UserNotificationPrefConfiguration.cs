using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserNotificationPref"/>. Maps to <c>USER_NOTIFICATION_PREFS</c>.</summary>
public sealed class UserNotificationPrefConfiguration : IEntityTypeConfiguration<UserNotificationPref>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserNotificationPref> builder)
    {
        builder.Property(x => x.EventType).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.ModuleCode, x.Channel, x.EventType }).IsUnique();

        builder.HasOne(x => x.User)
               .WithMany(u => u.NotificationPreferences)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
