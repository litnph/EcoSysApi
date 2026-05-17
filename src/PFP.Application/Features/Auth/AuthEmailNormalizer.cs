namespace PFP.Application.Features.Auth;

/// <summary>Normalises email addresses for storage and lookup (lower-case, trimmed).</summary>
public static class AuthEmailNormalizer
{
    /// <summary>Returns the canonical form stored in <c>USERS.email</c>.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
