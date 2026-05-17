namespace PFP.Domain.Enums;

/// <summary>
/// Direction of a <c>FIN_DEBT_RECORDS</c> row — does the platform user owe money,
/// or is money owed to them?
/// </summary>
public enum DebtDirection
{
    /// <summary>
    /// <c>borrowed</c> — the user borrowed money and owes it back.
    /// Created together with a <see cref="TransactionType.DebtBorrow"/> transaction.
    /// </summary>
    Borrowed = 1,

    /// <summary>
    /// <c>lent</c> — the user lent money and is waiting to be paid back.
    /// Created together with a <see cref="TransactionType.LoanGive"/> transaction.
    /// </summary>
    Lent = 2,
}
