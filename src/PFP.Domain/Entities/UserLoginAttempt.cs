namespace PFP.Domain.Entities;

/// <summary>
/// Append-only log of every authentication attempt. Maps to <c>USER_LOGIN_ATTEMPTS</c>.
/// <para>
/// Used by the brute-force-detection rule from security spec §6.1
/// (5 consecutive failures inside 15 min ⇒ lock the account for 15 min) — the lock decision
/// is recomputed from this log on every login attempt; no denormalised counter is kept on <see cref="User"/>.
/// </para>
/// <para>
/// <see cref="UserId"/> is nullable because failed attempts may target a non-existent email,
/// in which case <see cref="AttemptedEmail"/> is the only identifier we can record.
/// </para>
/// </summary>
public sealed class UserLoginAttempt : BaseEntity
{
    /// <summary>FK to <see cref="User"/>; <c>null</c> when the attempt targeted a non-existent account.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Email exactly as typed (lower-cased) — useful for analysing typo-squat patterns.</summary>
    public string AttemptedEmail { get; set; } = string.Empty;

    /// <summary><c>true</c> if the attempt issued a valid session.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Short machine-readable reason when <see cref="IsSuccess"/> is <c>false</c>:
    /// <c>invalid_password</c>, <c>account_locked</c>, <c>user_not_found</c>, <c>email_not_verified</c>, …
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>IP address as observed at the request time.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Raw User-Agent header.</summary>
    public string? UserAgent { get; set; }

    // ---- Navigation ----

    public User? User { get; set; }
}
