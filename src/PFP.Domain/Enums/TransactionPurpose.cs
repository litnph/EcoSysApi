namespace PFP.Domain.Enums;

/// <summary>Machine-readable business purpose independent of display text and localization.</summary>
public enum TransactionPurpose
{
    General = 1,
    StatementPayment = 2,
    InstallmentPayment = 3,
    ConversionFee = 4,
    SavingDeposit = 5,
    SavingWithdrawal = 6,
    Refund = 7,
}
