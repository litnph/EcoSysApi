using PFP.Application.Common.Exceptions;

namespace PFP.Application.Common;

/// <summary>Checks client-observed versions before mutating versioned financial aggregates.</summary>
public static class OptimisticConcurrencyGuard
{
    public static void Ensure(int actualVersion, int? expectedVersion)
    {
        if (expectedVersion is { } expected && expected != actualVersion)
            throw new ConcurrencyConflictException();
    }
}
