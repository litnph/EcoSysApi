using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.GetSourceBalanceLedger;

public sealed record GetSourceBalanceLedgerResponse(
    Guid SourceId,
    string SourceName,
    string Currency,
    long StoredBalance,
    long ComputedBalance,
    long Drift,
    IReadOnlyList<SourceBalanceLedgerEntryDto> Entries);
