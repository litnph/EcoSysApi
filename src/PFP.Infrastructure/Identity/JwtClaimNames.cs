namespace PFP.Infrastructure.Identity;

/// <summary>Custom JWT claim names used across handlers and middleware.</summary>
public static class JwtClaimNames
{
    /// <summary>Maps to <c>USER_SESSIONS.id</c>.</summary>
    public const string SessionId = "sid";

    /// <summary>Maps to the active <c>ORGANIZATIONS.id</c> selected for this access token.</summary>
    public const string OrgId = "org_id";
}
