namespace PFP.Domain.Enums;

/// <summary>Kind of append-only <c>fin_debt_transactions</c> row.</summary>
public enum DebtTxnType
{
    /// <summary><c>payment</c> — user paid down a borrowed debt.</summary>
    Payment = 1,

    /// <summary><c>collection</c> — user collected on a lent amount.</summary>
    Collection = 2,
}
