using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="FinTransactionHistory"/>. Maps to <c>fin_transaction_history</c>.</summary>
public sealed class FinTransactionHistoryConfiguration : IEntityTypeConfiguration<FinTransactionHistory>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinTransactionHistory> builder)
    {
        builder.Property(x => x.Snapshot).HasColumnType("jsonb");
        builder.Property(x => x.ChangedFields).HasColumnType("jsonb");
        builder.Property(x => x.ChangeReason).HasMaxLength(1024);

        builder.Property(x => x.TransactionId).HasColumnName("transaction_id");

        builder.HasIndex(x => new { x.TransactionId, x.Version });
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.TransactionId, x.CreatedAt });

        builder.HasOne(x => x.Transaction)
               .WithMany(t => t.History)
               .HasForeignKey(x => x.TransactionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
