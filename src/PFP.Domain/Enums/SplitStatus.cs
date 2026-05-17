namespace PFP.Domain.Enums;

/// <summary>
/// Settlement state for a single <c>fin_txn_splits</c> row (reimbursement from a participant).
/// </summary>
public enum SplitStatus
{
    /// <summary><c>pending</c> — the participant has not yet reimbursed.</summary>
    Pending = 1,

    /// <summary><c>settled</c> — reimbursement was recorded via an income transaction.</summary>
    Settled = 2,
}
