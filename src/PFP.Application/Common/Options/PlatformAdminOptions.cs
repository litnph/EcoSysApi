namespace PFP.Application.Common.Options;

/// <summary>Platform-level administrators allowed to manage translations / other global config.</summary>
public sealed class PlatformAdminOptions
{
    /// <summary>Configuration section key (<c>PlatformAdmin</c>).</summary>
    public const string SectionName = "PlatformAdmin";

    /// <summary>User ids (from JWT <c>sub</c>) that pass the <c>PlatformAdmin</c> authorization policy.</summary>
    public List<Guid> UserIds { get; set; } = new();
}
