using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations;

/// <summary>Maps <see cref="AutomationRule"/> to <c>automation_rules</c>.</summary>
public sealed class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TriggerValue).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Conditions).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Actions).HasColumnType("nvarchar(max)").IsRequired();

        builder.HasIndex(x => new { x.SmoduleId, x.IsActive });
        builder.HasIndex(x => new { x.TriggerType, x.IsActive });

        builder.HasOne(x => x.Smodule)
               .WithMany(m => m.AutomationRules)
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedBy)
               .WithMany(u => u.AutomationRulesCreated)
               .HasForeignKey(x => x.CreatedByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
