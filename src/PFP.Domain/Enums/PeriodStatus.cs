namespace PFP.Domain.Enums;

/// <summary>Lifecycle of a <c>FIN_MONTHLY_PERIODS</c> row (system-managed, no soft delete).</summary>
public enum PeriodStatus
{
    /// <summary><c>open</c> — aggregates are recomputed from transactions until the month is closed.</summary>
    Open = 1,

    /// <summary><c>closed</c> — totals frozen by <c>CloseMonth</c>; billing-cycle rules were satisfied.</summary>
    Closed = 2,
}
