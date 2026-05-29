using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Preview row for source balance / credit-limit reconciliation.</summary>
public sealed record SourceRecalculatePreviewItemDto(
    Guid SourceId,
    string Name,
    SourceType Type,
    string Currency,
    long StoredBalance,
    long ComputedBalance,
    long Drift,
    long? CreditLimit,
    decimal? StoredUtilizationPercent,
    decimal? ComputedUtilizationPercent);
