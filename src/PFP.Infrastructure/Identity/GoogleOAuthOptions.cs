namespace PFP.Infrastructure.Identity;

/// <summary>Binding target for <c>Google</c> OAuth configuration.</summary>
public sealed class GoogleOAuthOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Google";

    /// <summary>Google OAuth client id.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Google OAuth client secret.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}
