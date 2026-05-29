using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// Membership of a <see cref="FinTransaction"/> on a <see cref="FinBillingCycle"/> statement.
/// Maps to <c>fin_billing_cycle_items</c>.
/// </summary>
public sealed class FinBillingCycleItem : BaseEntity
{
    public Guid BillingCycleId { get; set; }

    public Guid TransactionId { get; set; }

    public BillingCycleItemInclusionSource InclusionSource { get; set; }

    /// <summary>When set, the line is excluded from statement totals (manual remove while cycle is open).</summary>
    public DateTime? RemovedAt { get; set; }

    public FinBillingCycle BillingCycle { get; set; } = null!;

    public FinTransaction Transaction { get; set; } = null!;

    public bool IsActive => RemovedAt is null;
}
