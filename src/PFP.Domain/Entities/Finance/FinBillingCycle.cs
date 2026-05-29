using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// One credit-card statement period (open → closed → paid / overdue). Maps to <c>FIN_BILLING_CYCLES</c>.
/// System-managed rows: no soft delete; lifecycle is driven by generate / close / pay handlers.
/// </summary>
public sealed class FinBillingCycle : BaseEntity
{
    /// <summary>FK to <see cref="FinSource"/> (must be <see cref="SourceType.CreditCard"/>).</summary>
    public Guid SourceId { get; set; }

    /// <summary>Display label, e.g. <c>Kỳ sao kê tháng 5</c> for period ending in May.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>First day of the spending period (inclusive).</summary>
    public DateOnly PeriodStart { get; set; }

    /// <summary>Last day of the spending period (inclusive).</summary>
    public DateOnly PeriodEnd { get; set; }

    /// <summary>Statement issue date for this cycle.</summary>
    public DateOnly StatementDate { get; set; }

    /// <summary>Payment due date (statement date + issuer grace days).</summary>
    public DateOnly PaymentDueDate { get; set; }

    /// <summary>Total charges posted to this cycle (sum of deferred spends while open / at close).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Amount paid toward this cycle.</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>Lifecycle status.</summary>
    public BillingCycleStatus Status { get; set; } = BillingCycleStatus.Open;

    /// <summary>UTC timestamp when the cycle was closed; <c>null</c> while <see cref="BillingCycleStatus.Open"/>.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>UTC timestamp when the cycle became fully paid.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>UTC timestamp of the last Refresh that mapped deferred transactions.</summary>
    public DateTime? LastRefreshedAt { get; set; }

    /// <summary>Optional note when <see cref="IssuerStatementAmount"/> differs from <see cref="TotalAmount"/>.</summary>
    public string? ReconciliationNote { get; set; }

    /// <summary>Total on the issuer statement (for reconciliation); optional.</summary>
    public decimal? IssuerStatementAmount { get; set; }

    // ---- Navigation ----
    public FinSource Source { get; set; } = null!;

    public ICollection<FinBillingCycleItem> Items { get; set; } = new List<FinBillingCycleItem>();
}
