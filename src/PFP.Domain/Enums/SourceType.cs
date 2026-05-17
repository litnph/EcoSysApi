namespace PFP.Domain.Enums;

/// <summary>
/// All recognised <c>FIN_SOURCES.type</c> values.
/// </summary>
public enum SourceType
{
    /// <summary><c>cash</c> — physical cash on hand.</summary>
    Cash = 1,

    /// <summary><c>bank_account</c> — checking / savings account at a bank.</summary>
    BankAccount = 2,

    /// <summary>
    /// <c>credit_card</c> — credit-card account.
    /// Required extra fields: <c>credit_limit</c>, <c>statement_day</c>, <c>payment_due_day</c>,
    /// optionally <c>min_installment_amt</c>. <c>balance</c> stores the outstanding (unpaid) amount.
    /// </summary>
    CreditCard = 3,

    /// <summary><c>e_wallet</c> — electronic wallet (MoMo, ZaloPay, ApplePay, …).</summary>
    EWallet = 4,

    /// <summary>
    /// <c>investment</c> — brokerage or fund account.
    /// Detailed positions live in <c>FIN_INVESTMENTS</c> / <c>FIN_INVESTMENT_TXN</c>.
    /// </summary>
    Investment = 5,
}
