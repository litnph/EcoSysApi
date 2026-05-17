namespace PFP.Domain.Entities;

/// <summary>Platform-wide feature toggle with optional rollout percentage and granular overrides (<c>FEATURE_FLAGS</c>).</summary>
public sealed class FeatureFlag : BaseEntity
{
    /// <summary>Stable programmatic key unique across the platform (max 100).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable title (max 200).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional admin notes (max 500).</summary>
    public string? Description { get; set; }

    /// <summary>When no override / rollout applies, this is the fallback.</summary>
    public bool IsEnabledGlobal { get; set; }

    /// <summary>Deterministic rollout bucket threshold 0–100; use 0 to disable rollout.</summary>
    public int RolloutPercentage { get; set; }

    /// <summary>Archived flags evaluate to disabled for callers.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Granular overrides (user/org).</summary>
    public ICollection<FeatureFlagOverride> Overrides { get; set; } = new List<FeatureFlagOverride>();
}
