using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="FinDebtRecordHistory"/>. Maps to <c>fin_debt_record_history</c>.</summary>
public sealed class FinDebtRecordHistoryConfiguration : VersionHistoryEntityConfiguration<FinDebtRecordHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<FinDebtRecordHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(d => d.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
