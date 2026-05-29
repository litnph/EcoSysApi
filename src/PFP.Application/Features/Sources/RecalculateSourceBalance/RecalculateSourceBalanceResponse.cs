namespace PFP.Application.Features.Sources.RecalculateSourceBalance;

public sealed record RecalculateSourceBalanceResponse(
    Guid SourceId,
    long PreviousBalance,
    long NewBalance);
