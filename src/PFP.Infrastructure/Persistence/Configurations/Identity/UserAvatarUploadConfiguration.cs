using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserAvatarUpload"/>. Maps to <c>USER_AVATAR_UPLOADS</c>.</summary>
public sealed class UserAvatarUploadConfiguration : IEntityTypeConfiguration<UserAvatarUpload>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserAvatarUpload> builder)
    {
        builder.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(x => x.StorageUrl).HasMaxLength(1024);
        builder.Property(x => x.ContentType).HasMaxLength(64).IsRequired();

        // Partial unique index: at most one active avatar per user (Postgres-specific filter).
        builder.HasIndex(x => x.UserId)
               .IsUnique()
               .HasFilter("[is_active] = 1")
               .HasDatabaseName("ix_user_avatar_uploads_user_id_active");

        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
               .WithMany(u => u.AvatarUploads)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
