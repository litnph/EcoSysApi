namespace PFP.Domain.Exceptions;

/// <summary>
/// Raised when a finance operation would drive a <c>FIN_SOURCES.balance</c> below zero
/// (or any negotiated minimum) and no fallback is in place.
/// <para>
/// Per spec §4.6 the balance never changes directly — it is always derived from a posted
/// <c>FIN_TRANSACTION</c>. The handler computes the resulting balance and throws this exception
/// before any row is added when the source would over-draw.
/// </para>
/// </summary>
public sealed class InsufficientBalanceException : DomainException
{
    /// <summary>Source whose balance was insufficient.</summary>
    public Guid SourceId { get; }

    /// <summary>Available balance at the moment of evaluation.</summary>
    public decimal Available { get; }

    /// <summary>Amount the operation tried to debit.</summary>
    public decimal Requested { get; }

    /// <summary>Creates the exception with diagnostic context.</summary>
    public InsufficientBalanceException(Guid sourceId, decimal available, decimal requested)
        : base($"Source {sourceId} has insufficient balance: available {available}, requested {requested}.")
    {
        SourceId = sourceId;
        Available = available;
        Requested = requested;
    }
}
