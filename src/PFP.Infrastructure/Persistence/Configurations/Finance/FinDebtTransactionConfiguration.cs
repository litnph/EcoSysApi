using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinDebtTransaction"/>. Maps to <c>fin_debt_transactions</c> (append-only).</summary>
public sealed class FinDebtTransactionConfiguration : IEntityTypeConfiguration<FinDebtTransaction>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinDebtTransaction> builder)
    {
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.TxnDate).HasColumnType("date");

        builder.HasIndex(x => new { x.DebtRecordId, x.TxnDate });

        builder.HasOne(x => x.DebtRecord)
               .WithMany(d => d.FinDebtTransactions)
               .HasForeignKey(x => x.DebtRecordId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Transaction)
               .WithMany()
               .HasForeignKey(x => x.TxnId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
