using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinSaving"/>. Maps to <c>FIN_SAVINGS</c>.</summary>
public sealed class FinSavingConfiguration : IEntityTypeConfiguration<FinSaving>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinSaving> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.InterestRate).HasPrecision(5, 4);

        builder.HasIndex(x => new { x.SmoduleId, x.Status });

        builder.HasOne(x => x.Smodule)
               .WithMany(m => m.FinSavings)
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Source)
               .WithMany()
               .HasForeignKey(x => x.SourceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
