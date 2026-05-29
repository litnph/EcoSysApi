using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// One calendar month of finance activity for a space-module. Maps to <c>FIN_MONTHLY_PERIODS</c>.
/// System-managed: <b>no soft delete</b> (inherits <see cref="BaseEntity"/> only).
/// </summary>
public sealed class FinMonthlyPeriod : BaseEntity
{
    /// <summary>Calendar year (e.g. <c>2026</c>).</summary>
    public int Year { get; set; }

    /// <summary>Calendar month <c>1</c>–<c>12</c>.</summary>
    public int Month { get; set; }

    /// <summary>Sum of income-class transactions for the month after close (also refreshed on close).</summary>
    public decimal TotalIncome { get; set; }

    /// <summary>Sum of expense-class transactions for the month after close (also refreshed on close).</summary>
    public decimal TotalExpense { get; set; }

    /// <summary><see cref="TotalIncome"/> − <see cref="TotalExpense"/> (persisted on close).</summary>
    public decimal Net { get; set; }

    /// <summary>Open vs closed month.</summary>
    public PeriodStatus Status { get; set; } = PeriodStatus.Open;

    /// <summary>UTC timestamp when the month was closed; <c>null</c> while <see cref="PeriodStatus.Open"/>.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>User who closed the month.</summary>
    public Guid? ClosedBy { get; set; }

    /// <summary>JSON array: category expense breakdown at close (<c>jsonb</c>).</summary>
    public string? CategoryBreakdown { get; set; }

    /// <summary>JSON array: per-source expense totals at close (<c>jsonb</c>).</summary>
    public string? SourceBreakdown { get; set; }

    /// <summary>UTC when the user explicitly created the monthly report.</summary>
    public DateTime? ReportCreatedAt { get; set; }

    /// <summary>UTC when report data was last refreshed while still open.</summary>
    public DateTime? LastRefreshedAt { get; set; }

    /// <summary>Full monthly report JSON snapshot (refreshed while open, frozen on close).</summary>
    public string? ReportSnapshot { get; set; }

    // ---- Navigation ----
    public User? ClosedByUser { get; set; }
}
