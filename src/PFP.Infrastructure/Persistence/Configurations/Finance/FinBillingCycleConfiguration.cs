using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinBillingCycle"/>. Maps to <c>fin_billing_cycles</c> (spec <c>FIN_BILLING_CYCLES</c>).</summary>
public sealed class FinBillingCycleConfiguration : IEntityTypeConfiguration<FinBillingCycle>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinBillingCycle> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();

        builder.Property(x => x.PeriodStart).HasColumnType("date");
        builder.Property(x => x.PeriodEnd).HasColumnType("date");
        builder.Property(x => x.StatementDate).HasColumnType("date");
        builder.Property(x => x.PaymentDueDate).HasColumnType("date");

        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.IssuerStatementAmount).HasPrecision(18, 2);
        builder.Property(x => x.ReconciliationNote).HasMaxLength(500);

        builder.HasIndex(x => new { x.SourceId, x.Status });
        builder.HasIndex(x => new { x.SourceId, x.PeriodStart, x.PeriodEnd });
        builder.HasIndex(x => new { x.SourceId, x.PeriodStart }).IsUnique();
        builder.HasOne(x => x.Source)
               .WithMany(s => s.BillingCycles)
               .HasForeignKey(x => x.SourceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
               .WithOne(i => i.BillingCycle)
               .HasForeignKey(i => i.BillingCycleId);
    }
}
