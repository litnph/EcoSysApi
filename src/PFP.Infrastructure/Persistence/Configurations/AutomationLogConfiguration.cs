using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AutomationLog"/> to <c>automation_logs</c>.</summary>
public sealed class AutomationLogConfiguration : IEntityTypeConfiguration<AutomationLog>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AutomationLog> builder)
    {
        builder.Property(x => x.ActionsExecuted).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);

        builder.HasIndex(x => new { x.RuleId, x.TriggeredAt });

        builder.HasOne(x => x.Rule)
               .WithMany(r => r.Logs)
               .HasForeignKey(x => x.RuleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
