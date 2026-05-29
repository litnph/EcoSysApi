using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Helpers for asset sources (balance ledger / adjustments apply; credit cards use limit instead).</summary>
public static class AssetSourceRules
{
    /// <summary>True when the source tracks available funds via balance ledger (not credit-card debt).</summary>
    public static bool SupportsBalanceLedger(SourceType type) =>
        type != SourceType.CreditCard;
}
