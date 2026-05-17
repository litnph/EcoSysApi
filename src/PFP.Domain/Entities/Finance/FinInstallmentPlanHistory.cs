using PFP.Domain.Entities;

namespace PFP.Domain.Entities.Finance;

/// <summary>Version history for <see cref="FinInstallmentPlan"/>. Maps to <c>fin_installment_plan_history</c>.</summary>
public sealed class FinInstallmentPlanHistory : VersionHistoryEntity
{
    public Guid EntityId { get; set; }

    public FinInstallmentPlan Entity { get; set; } = null!;
}
