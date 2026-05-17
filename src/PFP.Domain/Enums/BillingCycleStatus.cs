namespace PFP.Domain.Enums;

/// <summary>
/// Lifecycle of a <c>FIN_BILLING_CYCLES</c> row (per spec §3.6).
/// <para>
/// Billing cycles are <b>system-managed</b>: created by the <c>GenerateBillingCycles</c> Hangfire
/// job (spec §7.1), closed by the user once the credit-card statement arrives, and marked
/// <see cref="Paid"/> when the linked payment transaction posts. Users cannot delete cycles.
/// </para>
/// </summary>
public enum BillingCycleStatus
{
    /// <summary><c>open</c> — the cycle is currently accruing spend; new <see cref="TransactionType.Deferred"/> rows attach here.</summary>
    Open = 1,

    /// <summary><c>closed</c> — statement date reached; <c>closing_balance</c> frozen, awaiting payment.</summary>
    Closed = 2,

    /// <summary><c>paid</c> — outstanding balance settled in full by a payment transaction.</summary>
    Paid = 3,

    /// <summary><c>overdue</c> — payment due date passed without full settlement; flagged by the <c>CheckOverdueCycles</c> job.</summary>
    Overdue = 4,
}
