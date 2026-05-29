namespace PFP.Domain.Enums;

/// <summary>How a transaction was linked to a billing cycle statement.</summary>
public enum BillingCycleItemInclusionSource
{
    /// <summary>Matched by Refresh from card period rules.</summary>
    Refresh = 1,

    /// <summary>User explicitly added to the statement.</summary>
    ManualAdd = 2,
}
