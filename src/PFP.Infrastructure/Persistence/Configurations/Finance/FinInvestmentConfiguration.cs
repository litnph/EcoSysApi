using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinInvestment"/>. Maps to <c>FIN_INVESTMENTS</c>.</summary>
public sealed class FinInvestmentConfiguration : IEntityTypeConfiguration<FinInvestment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinInvestment> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => new { x.SmoduleId, x.Type });

        builder.HasOne(x => x.Smodule)
               .WithMany(m => m.FinInvestments)
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
