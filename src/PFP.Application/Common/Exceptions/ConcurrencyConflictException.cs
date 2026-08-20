namespace PFP.Application.Common.Exceptions;

/// <summary>Raised when a caller attempts to mutate a stale aggregate version.</summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The financial record changed since it was loaded. Refresh and retry.")
    {
    }
}
