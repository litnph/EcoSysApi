namespace PFP.Domain.Enums;

/// <summary>Audience for a <see cref="Entities.FeatureFlagOverride"/> row.</summary>
public enum OverrideTargetType
{
    /// <summary>Single user (<c>USERS.id</c>).</summary>
    User = 0,

    /// <summary>Entire organisation (<c>ORGANIZATIONS.id</c>).</summary>
    Org = 1,
}
