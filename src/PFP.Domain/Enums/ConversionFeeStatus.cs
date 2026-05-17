namespace PFP.Domain.Enums;

/// <summary>
/// Status of the conversion fee charged when a credit-card transaction is converted into an
/// installment plan (spec §4.3).
/// </summary>
public enum ConversionFeeStatus
{
    /// <summary><c>pending</c> — plan created but the conversion-fee transaction has not yet been emitted.</summary>
    Pending = 1,

    /// <summary><c>billed</c> — the next billing cycle has opened and a <c>FIN_TRANSACTIONS</c> row for the fee was emitted.</summary>
    Billed = 2,

    /// <summary><c>paid</c> — the billing cycle containing the conversion-fee transaction has been settled.</summary>
    Paid = 3,
}
