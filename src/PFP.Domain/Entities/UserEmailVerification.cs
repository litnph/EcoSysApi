using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Append-only token ledger for email verification, covering both the initial verification of a new
/// account and the change-email confirmation flow. Maps to <c>USER_EMAIL_VERIFICATIONS</c>.
/// <para>
/// As with password resets, only the SHA-256 hash of the token is stored, and a row is single-use.
/// Default token TTL per registration spec §4.1 is 24 hours.
/// </para>
/// </summary>
public sealed class UserEmailVerification : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Discriminates between the two flows that share this table.</summary>
    public EmailVerificationType Type { get; set; }

    /// <summary>SHA-256 hash of the raw verification token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Hard expiry of the token (default: <c>now + 24 hours</c>).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC timestamp at which the token was redeemed; <c>null</c> while still usable.</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Required when <see cref="Type"/> is <see cref="EmailVerificationType.ChangeEmail"/>:
    /// the new address that will replace <c>USERS.email</c> on confirmation.
    /// Always <c>null</c> for <see cref="EmailVerificationType.VerifyEmail"/>.
    /// </summary>
    public string? NewEmail { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
