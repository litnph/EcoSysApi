using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF mapping for <see cref="FinBillingCycleItem"/> → <c>fin_billing_cycle_items</c>.</summary>
public sealed class FinBillingCycleItemConfiguration : IEntityTypeConfiguration<FinBillingCycleItem>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinBillingCycleItem> builder)
    {
        builder.HasIndex(x => x.BillingCycleId);
        builder.HasIndex(x => x.TransactionId)
            .IsUnique()
            .HasFilter("[removed_at] IS NULL")
            .HasDatabaseName("ux_fin_billing_cycle_items_transaction_id_active");

        builder.HasOne(x => x.BillingCycle)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.BillingCycleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Transaction)
            .WithMany(t => t.BillingCycleItems)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
