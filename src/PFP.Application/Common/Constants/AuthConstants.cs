namespace PFP.Application.Common.Constants;

/// <summary>Centralised auth-related numeric and time limits from the platform security spec.</summary>
public static class AuthConstants
{
    /// <summary>Bcrypt work factor for <c>USERS.password_hash</c> (spec §6.1).</summary>
    public const int BcryptWorkFactor = 12;

    /// <summary>Lifetime of JWT access tokens in minutes (spec §6.1).</summary>
    public const int AccessTokenLifetimeMinutes = 15;

    /// <summary>Lifetime of refresh tokens in days (spec §6.1).</summary>
    public const int RefreshTokenLifetimeDays = 30;

    /// <summary>Initial email-verification token TTL (spec §4.1).</summary>
    public const int EmailVerificationTokenLifetimeHours = 24;

    /// <summary>Password-reset token TTL (spec §6.1).</summary>
    public const int PasswordResetTokenLifetimeHours = 1;

    /// <summary>Brute-force window: failed attempts inside this span count toward a lock (spec §6.1).</summary>
    public static readonly TimeSpan LoginFailureWindow = TimeSpan.FromMinutes(15);

    /// <summary>How many failures inside <see cref="LoginFailureWindow"/> trigger a temporary lock.</summary>
    public const int MaxFailedLoginAttemptsInWindow = 5;

    /// <summary>How long the account stays locked after the threshold is reached (spec §6.1).</summary>
    public static readonly TimeSpan AccountLockDuration = TimeSpan.FromMinutes(15);

    /// <summary>Email link TTL for GDPR account-deletion confirmation.</summary>
    public const int AccountDeletionConfirmationTokenLifetimeHours = 24;

    /// <summary>Grace period after confirmed deletion (spec §3.4 / GDPR flow).</summary>
    public const int AccountDeletionGracePeriodDays = 30;
}
