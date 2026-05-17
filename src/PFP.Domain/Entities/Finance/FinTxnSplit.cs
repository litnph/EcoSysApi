using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// One participant share of a <see cref="TransactionType.Split"/> transaction. Maps to <c>fin_txn_splits</c>.
/// </summary>
public sealed class FinTxnSplit : SoftDeletableEntity
{
    /// <summary>FK to the parent <see cref="FinTransaction"/> with <see cref="TransactionType.Split"/>.</summary>
    public Guid TransactionId { get; set; }

    /// <summary>Display name of the person who owes reimbursement.</summary>
    public string PersonName { get; set; } = string.Empty;

    /// <summary>Optional contact (phone / email).</summary>
    public string? PersonContact { get; set; }

    /// <summary>Amount this person should pay back (positive).</summary>
    public decimal Amount { get; set; }

    public SplitStatus Status { get; set; } = SplitStatus.Pending;

    /// <summary>UTC when <see cref="Status"/> became <see cref="SplitStatus.Settled"/>.</summary>
    public DateTime? SettledAt { get; set; }

    /// <summary>FK to the income transaction posted when this split was settled.</summary>
    public Guid? SettledTxnId { get; set; }

    public FinTransaction Transaction { get; set; } = null!;

    public FinTransaction? SettledTransaction { get; set; }
}
