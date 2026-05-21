namespace PFP.Domain.Entities;

/// <summary>
/// Binary asset stored in object storage, linked to a finance entity (<c>file_attachments</c>).
/// </summary>
public sealed class FileAttachment : SoftDeletableEntity
{
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

    /// <summary>Uploader (<c>users.id</c>).</summary>
    public Guid UploadedBy { get; set; }

    /// <summary>Hints public readability (API still checks parent-entity ACL).</summary>
    public bool IsPublic { get; set; }

    public User Uploader { get; set; } = null!;
}
