namespace PFP.Application.Common.Exceptions;

/// <summary>Raised when an idempotency key is reused for a materially different financial command.</summary>
public sealed class IdempotencyConflictException : Exception
{
    public IdempotencyConflictException()
        : base("ClientRequestId was already used for a different transaction payload.")
    {
    }
}
