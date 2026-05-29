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
        builder.HasIndex(x => new { x.Year, x.Month }).IsUnique();
        builder.Property(x => x.CategoryBreakdown).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SourceBreakdown).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ReportSnapshot).HasColumnType("nvarchar(max)");
        builder.HasOne(x => x.ClosedByUser)
               .WithMany()
               .HasForeignKey(x => x.ClosedBy)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
