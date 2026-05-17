using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Audit;

/// <summary>EF Core mapping for <see cref="AuditLogRetention"/>. Maps to <c>AUDIT_LOG_RETENTIONS</c>.</summary>
public sealed class AuditLogRetentionConfiguration : IEntityTypeConfiguration<AuditLogRetention>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AuditLogRetention> builder)
    {
        builder.Property(x => x.EntityType).HasMaxLength(128);
        builder.Property(x => x.ArchiveStorageKeyPrefix).HasMaxLength(512);

        // At most one policy per entity-type (NULL is the catch-all default).
        builder.HasIndex(x => x.EntityType).IsUnique();
    }
}
