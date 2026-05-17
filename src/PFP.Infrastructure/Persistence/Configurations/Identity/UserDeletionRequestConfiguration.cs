using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserDeletionRequest"/>. Maps to <c>USER_DELETION_REQUESTS</c>.</summary>
public sealed class UserDeletionRequestConfiguration : IEntityTypeConfiguration<UserDeletionRequest>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserDeletionRequest> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(2048);
        builder.Property(x => x.ConfirmationTokenHash).HasMaxLength(64);

        builder.HasIndex(x => x.ConfirmationTokenHash);
        builder.HasIndex(x => new { x.Status, x.ScheduledExecutionAt });
        builder.HasIndex(x => new { x.UserId, x.Status });

        builder.HasOne(x => x.User)
               .WithMany(u => u.DeletionRequests)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
