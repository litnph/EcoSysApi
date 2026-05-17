namespace PFP.Domain.Entities;

/// <summary>
/// Avatar image upload history for a <see cref="User"/>. Maps to <c>USER_AVATAR_UPLOADS</c>.
/// <para>
/// At most one row per user has <see cref="IsActive"/> = <c>true</c> at any moment — enforced by a
/// PostgreSQL partial unique index on <c>(user_id) WHERE is_active = true</c>, configured in the EF mapping.
/// Deactivated rows are kept so we can restore a previous avatar without re-uploading.
/// </para>
/// </summary>
public sealed class UserAvatarUpload : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Object key inside Cloudflare R2 / S3 bucket.</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>Public CDN URL (or pre-signed URL) used by the front-end.</summary>
    public string? StorageUrl { get; set; }

    /// <summary>MIME type as reported on upload, e.g. <c>image/jpeg</c>, <c>image/png</c>.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Size of the stored object in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary><c>true</c> for the avatar currently displayed; only one row per user may carry this flag.</summary>
    public bool IsActive { get; set; } = true;

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
