namespace PFP.Domain.Enums;

/// <summary>Lifecycle status stored on <c>FIN_TRANSACTIONS.status</c> (MVP subset).</summary>
public enum TxnStatus
{
    /// <summary><c>pending</c></summary>
    Pending = 1,

    /// <summary><c>completed</c></summary>
    Completed = 2,

    /// <summary><c>cancelled</c></summary>
    Cancelled = 3,
}
