namespace PFP.Domain.Entities;

/// <summary>
/// Append-only token ledger for the "forgot password" flow. Maps to <c>USER_PASSWORD_RESETS</c>.
/// <para>
/// Per security spec §6.1: only the SHA-256 hash of the reset token is stored; tokens expire after 1 hour
/// and must be marked single-use by setting <see cref="UsedAt"/> the moment the new password is accepted.
/// </para>
/// </summary>
public sealed class UserPasswordReset : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw reset token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Hard expiry of the token (default: <c>now + 1 hour</c>).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC timestamp at which the token was redeemed; <c>null</c> while still usable.</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>IP address that requested the reset link (forensics).</summary>
    public string? RequestIpAddress { get; set; }

    /// <summary>IP address that completed the reset (forensics).</summary>
    public string? UsedIpAddress { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
