namespace PFP.Application.Common.Constants;

/// <summary>Centralised auth-related numeric and time limits from the platform security spec.</summary>
public static class AuthConstants
{
    /// <summary>Bcrypt work factor for <c>users.password_hash</c> (spec §6.1).</summary>
    public const int BcryptWorkFactor = 12;

    /// <summary>Lifetime of JWT access tokens in minutes (8 hours).</summary>
    public const int AccessTokenLifetimeMinutes = 8 * 60;

    /// <summary>Lifetime of refresh tokens in days (spec §6.1).</summary>
    public const int RefreshTokenLifetimeDays = 30;
}
