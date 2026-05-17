using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Audit;

/// <summary>
/// EF Core mapping for <see cref="AuditLog"/>. Maps to <c>AUDIT_LOGS</c>.
/// <para>
/// Composite indexes per spec §3.8:
/// (<c>EntityType, EntityId, CreatedAt</c>) and (<c>UserId, CreatedAt</c>).
/// </para>
/// <para>JSON columns are mapped to PostgreSQL <c>jsonb</c> for indexable / queryable payloads.</para>
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        builder.Property(x => x.BeforeSnapshot).HasColumnType("jsonb");
        builder.Property(x => x.AfterSnapshot).HasColumnType("jsonb");
        builder.Property(x => x.ChangedFields).HasColumnType("jsonb");

        // Spec §3.8 indexes.
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
