using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Identity;

/// <summary>EF Core mapping for <see cref="UserDataExport"/>. Maps to <c>USER_DATA_EXPORTS</c>.</summary>
public sealed class UserDataExportConfiguration : IEntityTypeConfiguration<UserDataExport>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<UserDataExport> builder)
    {
        builder.Property(x => x.StorageKey).HasMaxLength(512);
        builder.Property(x => x.DownloadUrl).HasMaxLength(2048);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2048);

        // The processor worker scans (status, ExpiresAt).
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.Status });

        builder.HasOne(x => x.User)
               .WithMany(u => u.DataExports)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
