using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinTxnSplit"/>. Maps to <c>fin_txn_splits</c>.</summary>
public sealed class FinTxnSplitConfiguration : IEntityTypeConfiguration<FinTxnSplit>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinTxnSplit> builder)
    {
        builder.Property(x => x.PersonName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PersonContact).HasMaxLength(200);

        builder.HasIndex(x => new { x.TransactionId, x.Status });

        builder.HasOne(x => x.Transaction)
               .WithMany(t => t.Splits)
               .HasForeignKey(x => x.TransactionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SettledTransaction)
               .WithMany()
               .HasForeignKey(x => x.SettledTxnId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
