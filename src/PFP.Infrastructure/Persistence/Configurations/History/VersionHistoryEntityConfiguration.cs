using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>
/// Shared configuration for every <c>*_HISTORY</c> table.
/// <para>
/// Implements the spec §3.7 schema (JSON snapshot + JSON changed_fields, change reason) and the
/// spec §3.8 composite index <c>(EntityId, Version)</c>. Concrete subclasses add the FK to the
/// parent entity (which is type-specific).
/// </para>
/// </summary>
/// <typeparam name="THistory">Concrete history entity type, derived from <see cref="VersionHistoryEntity"/>.</typeparam>
public abstract class VersionHistoryEntityConfiguration<THistory> : IEntityTypeConfiguration<THistory>
    where THistory : VersionHistoryEntity
{
    /// <inheritdoc/>
    public virtual void Configure(EntityTypeBuilder<THistory> builder)
    {
        builder.Property(x => x.Snapshot).HasColumnType("jsonb");
        builder.Property(x => x.ChangedFields).HasColumnType("jsonb");
        builder.Property(x => x.ChangeReason).HasMaxLength(1024);

        // Spec §3.8: (entity_id, version).
        builder.HasIndex("EntityId", nameof(VersionHistoryEntity.Version));
        builder.HasIndex(nameof(BaseEntity.CreatedAt));
        builder.HasIndex("EntityId", nameof(BaseEntity.CreatedAt));
    }
}
