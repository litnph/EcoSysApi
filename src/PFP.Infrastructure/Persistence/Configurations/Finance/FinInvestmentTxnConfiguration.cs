using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinInvestmentTxn"/>. Maps to <c>FIN_INVESTMENT_TXN</c> (append-only).</summary>
public sealed class FinInvestmentTxnConfiguration : IEntityTypeConfiguration<FinInvestmentTxn>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinInvestmentTxn> builder)
    {
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.PricePerUnit).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.InvestmentId, x.TxnDate });
        builder.HasIndex(x => x.LinkedTxnId);

        builder.HasOne(x => x.Investment)
               .WithMany(i => i.InvestmentTxns)
               .HasForeignKey(x => x.InvestmentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.LinkedTxn)
               .WithMany()
               .HasForeignKey(x => x.LinkedTxnId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
