namespace PFP.Domain.Entities;

/// <summary>
/// Binary asset stored in S3-compatible object storage, linked to a domain entity (<c>FILE_ATTACHMENTS</c>).
/// </summary>
public sealed class FileAttachment : SoftDeletableEntity
{
    /// <summary>Module discriminator (e.g. <c>finance</c>) — max 50.</summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>CLR-like entity label (e.g. <see cref="FinTransaction"/>).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of <see cref="EntityType"/>.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Original display name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Object key in R2/S3.</summary>
    public string FileKey { get; set; } = string.Empty;

    /// <summary>MIME content type supplied at upload.</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Size in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>Uploader (<c>USERS.id</c>).</summary>
    public Guid UploadedBy { get; set; }

    /// <summary>Hints public readability (API still checks parent-entity ACL).</summary>
    public bool IsPublic { get; set; }

    /// <inheritdoc cref="UploadedBy"/>
    public User Uploader { get; set; } = null!;
}
