using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.Persistence.Configurations.Finance;

/// <summary>EF Core mapping for <see cref="FinMonthlyPeriod"/>. Maps to <c>FIN_MONTHLY_PERIODS</c>.</summary>
public sealed class FinMonthlyPeriodConfiguration : IEntityTypeConfiguration<FinMonthlyPeriod>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<FinMonthlyPeriod> builder)
    {
        builder.HasIndex(x => new { x.SmoduleId, x.Year, x.Month }).IsUnique();
        builder.HasIndex(x => new { x.SmoduleId, x.Status });

        builder.Property(x => x.CategoryBreakdown).HasColumnType("jsonb");
        builder.Property(x => x.SourceBreakdown).HasColumnType("jsonb");

        builder.HasOne(x => x.Smodule)
               .WithMany(m => m.FinMonthlyPeriods)
               .HasForeignKey(x => x.SmoduleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ClosedByUser)
               .WithMany()
               .HasForeignKey(x => x.ClosedBy)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
