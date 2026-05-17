using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinInstallmentPay"/>. Maps to <c>fin_installment_pays</c>.</summary>
public sealed class FinInstallmentPayConfiguration : IEntityTypeConfiguration<FinInstallmentPay>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinInstallmentPay> builder)
    {
        builder.Property(x => x.DueDate).HasColumnType("date");
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.Property(x => x.PlanId).HasColumnName("installment_plan_id");
        builder.Property(x => x.InstallmentNumber).HasColumnName("installment_number");
        builder.Property(x => x.TxnId).HasColumnName("transaction_id");

        builder.HasIndex(x => new { x.PlanId, x.InstallmentNumber }).IsUnique();
        builder.HasIndex(x => new { x.PlanId, x.Status });

        builder.HasOne(x => x.Plan)
               .WithMany(p => p.Pays)
               .HasForeignKey(x => x.PlanId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Transaction)
               .WithMany()
               .HasForeignKey(x => x.TxnId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
