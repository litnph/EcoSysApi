using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// GDPR "download my data" request. Maps to <c>USER_DATA_EXPORTS</c>.
/// <para>
/// Asynchronously processed by the <c>ProcessDataExports</c> Hangfire job (every 5 minutes).
/// On completion the worker uploads a ZIP archive to Cloudflare R2, fills <see cref="StorageKey"/>
/// and <see cref="DownloadUrl"/>, and transitions the <see cref="Status"/> to
/// <see cref="DataExportStatus.Ready"/>; once <see cref="ExpiresAt"/> elapses, the same job moves
/// the row to <see cref="DataExportStatus.Expired"/> and purges the archive.
/// </para>
/// </summary>
public sealed class UserDataExport : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Current step of the export workflow.</summary>
    public DataExportStatus Status { get; set; } = DataExportStatus.Pending;

    /// <summary>UTC timestamp at which the worker picked the row up.</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>UTC timestamp at which the archive became downloadable.</summary>
    public DateTime? ReadyAt { get; set; }

    /// <summary>UTC timestamp at which the download link stops working.</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Object key inside Cloudflare R2 / S3 bucket.</summary>
    public string? StorageKey { get; set; }

    /// <summary>Pre-signed download URL handed to the user; expires together with <see cref="ExpiresAt"/>.</summary>
    public string? DownloadUrl { get; set; }

    /// <summary>Size of the produced archive in bytes.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>Last error message recorded if the export failed.</summary>
    public string? ErrorMessage { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
