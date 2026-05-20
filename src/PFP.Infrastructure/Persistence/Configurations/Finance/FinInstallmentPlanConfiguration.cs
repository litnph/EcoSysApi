using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinInstallmentPlan"/>. Maps to <c>fin_installment_plans</c>.</summary>
public sealed class FinInstallmentPlanConfiguration : IEntityTypeConfiguration<FinInstallmentPlan>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinInstallmentPlan> builder)
    {
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.MonthlyAmount).HasPrecision(18, 2);
        builder.Property(x => x.InterestRate).HasPrecision(5, 4);
        builder.Property(x => x.ConversionFeeRate).HasPrecision(5, 4);
        builder.Property(x => x.ConversionFeeAmount).HasPrecision(18, 2);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.CancellationReason).HasMaxLength(1024);

        builder.Property(x => x.OriginalTxnId).HasColumnName("origin_transaction_id");        builder.HasIndex(x => x.OriginalTxnId);
        builder.HasIndex(x => x.SourceId);
        builder.HasOne(x => x.Source)
               .WithMany(s => s.InstallmentPlans)
               .HasForeignKey(x => x.SourceId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OriginalTransaction)
               .WithMany()
               .HasForeignKey(x => x.OriginalTxnId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ConversionFeeTxn)
               .WithMany()
               .HasForeignKey(x => x.ConversionFeeTxnId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
