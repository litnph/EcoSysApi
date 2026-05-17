namespace PFP.Infrastructure.Identity;

/// <summary>Authentication scheme names used for the Google OAuth round-trip.</summary>
public static class GoogleAuthConstants
{
    /// <summary>Short-lived external cookie issued after Google redirects back to the API.</summary>
    public const string ExternalCookieScheme = "Google.External";
}
