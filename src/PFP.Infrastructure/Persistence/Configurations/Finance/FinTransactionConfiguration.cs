using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinTransaction"/>. Maps to <c>FIN_TRANSACTIONS</c>.</summary>
public sealed class FinTransactionConfiguration : IEntityTypeConfiguration<FinTransaction>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinTransaction> builder)
    {
        builder.Property(x => x.TxnDate).HasColumnType("date");
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.CounterpartyName).HasMaxLength(255);
        builder.Property(x => x.ExternalRef).HasMaxLength(255);
        builder.Property(x => x.Tags).HasMaxLength(1024);
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);        builder.HasIndex(x => x.BillingCycleId);
        builder.HasIndex(x => x.RefTxnId);
        builder.HasIndex(x => x.SourceId);
        builder.HasIndex(x => new { x.SourceId, x.TxnDate });        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.InstallmentPlanId);
        builder.HasOne(x => x.Source)
               .WithMany(s => s.Transactions)
               .HasForeignKey(x => x.SourceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestSource)
               .WithMany()
               .HasForeignKey(x => x.DestSourceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
               .WithMany(c => c.Transactions)
               .HasForeignKey(x => x.CategoryId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BillingCycle)
               .WithMany(bc => bc.Transactions)
               .HasForeignKey(x => x.BillingCycleId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.MonthlyPeriod)
               .WithMany()
               .HasForeignKey(x => x.MonthlyPeriodId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RefTransaction)
               .WithMany(t => t.RelatedTransactions)
               .HasForeignKey(x => x.RefTxnId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InstallmentPlan)
               .WithMany()
               .HasForeignKey(x => x.InstallmentPlanId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
