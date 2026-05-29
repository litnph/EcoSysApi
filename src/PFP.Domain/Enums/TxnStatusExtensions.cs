namespace PFP.Domain.Enums;

/// <summary>Helpers for <see cref="TxnStatus"/>.</summary>
public static class TxnStatusExtensions
{
    /// <summary>Whether the row still affects source balances and ledgers.</summary>
    public static bool AffectsLedger(this TxnStatus status) => status != TxnStatus.Cancelled;
}
