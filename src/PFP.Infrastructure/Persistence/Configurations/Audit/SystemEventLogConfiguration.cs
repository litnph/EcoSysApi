using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Audit;

/// <summary>EF Core mapping for <see cref="SystemEventLog"/>. Maps to <c>SYSTEM_EVENT_LOGS</c> (append-only).</summary>
public sealed class SystemEventLogConfiguration : IEntityTypeConfiguration<SystemEventLog>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<SystemEventLog> builder)
    {
        builder.Property(x => x.EventType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(128);
        builder.Property(x => x.JobName).HasMaxLength(255);
        builder.Property(x => x.JobId).HasMaxLength(64);
        builder.Property(x => x.Status).HasMaxLength(16).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2048);

        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.StackTrace).HasColumnType("text");

        builder.HasIndex(x => new { x.EventType, x.CreatedAt });
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });
        builder.HasIndex(x => new { x.JobName, x.CreatedAt });
    }
}
