using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>Per-user or per-org override attached to one <see cref="FeatureFlag"/> (<c>FEATURE_FLAG_OVERRIDES</c>).</summary>
public sealed class FeatureFlagOverride : BaseEntity
{
    /// <summary>FK to <see cref="FeatureFlag"/>.</summary>
    public Guid FlagId { get; set; }

    /// <summary>Selects whether <see cref="TargetId"/> is a user id or organisation id.</summary>
    public OverrideTargetType TargetType { get; set; }

    /// <summary><see cref="OverrideTargetType.User"/> → <c>users.id</c>; <see cref="OverrideTargetType.Org"/> → <c>organizations.id</c>.</summary>
    public Guid TargetId { get; set; }

    /// <summary>Effective enablement before considering expired rows.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>UTC instant after which the override is ignored; <c>null</c> means never expires.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Parent toggle.</summary>
    public FeatureFlag Flag { get; set; } = null!;
}
