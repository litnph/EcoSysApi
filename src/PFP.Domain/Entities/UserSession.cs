namespace PFP.Domain.Entities;

/// <summary>
/// Represents one issued refresh token and its associated device context.
/// Maps to <c>USER_SESSIONS</c>.
/// <para>
/// Per security spec §6.1: the raw refresh token is NEVER stored — only its SHA-256 hash
/// in <see cref="TokenHash"/>. Access tokens (15 min JWTs) are stateless and not persisted here;
/// the session row is what makes a refresh token revocable.
/// </para>
/// </summary>
public sealed class UserSession : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the raw refresh token; the unique index lives on this column.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Hard expiry of the refresh token (registration default: <c>now + 30 days</c>).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the user logs out or an admin force-revokes the session. Null while active.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>UTC timestamp of the latest <c>POST /auth/refresh</c> served from this session.</summary>
    public DateTime LastUsedAt { get; set; }

    /// <summary>
    /// Organisation context for JWT <c>org_id</c> on refresh. When null, the user's personal org is used.
    /// </summary>
    public Guid? ActiveOrgId { get; set; }

    // ---- Device info (best-effort, populated from request headers) ----

    /// <summary>Friendly device label, e.g. "iPhone 15 Pro" or "Chrome on Windows".</summary>
    public string? DeviceName { get; set; }

    /// <summary>Coarse classification: <c>web</c>, <c>mobile</c>, <c>tablet</c>, <c>desktop</c>.</summary>
    public string? DeviceType { get; set; }

    /// <summary>Raw User-Agent header as received at the session-creation time.</summary>
    public string? UserAgent { get; set; }

    /// <summary>IP address as observed at the session-creation time (forensics).</summary>
    public string? IpAddress { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
