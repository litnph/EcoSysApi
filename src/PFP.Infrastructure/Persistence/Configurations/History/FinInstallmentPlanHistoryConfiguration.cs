using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PFP.Domain.Entities.Finance;

namespace PFP.Infrastructure.Persistence.Configurations.History;

/// <summary>EF Core mapping for <see cref="FinInstallmentPlanHistory"/>. Maps to <c>fin_installment_plan_history</c>.</summary>
public sealed class FinInstallmentPlanHistoryConfiguration : VersionHistoryEntityConfiguration<FinInstallmentPlanHistory>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<FinInstallmentPlanHistory> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Entity)
               .WithMany(p => p.History)
               .HasForeignKey(x => x.EntityId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
