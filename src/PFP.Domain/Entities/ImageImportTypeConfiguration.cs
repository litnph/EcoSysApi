namespace PFP.Domain.Entities;

/// <summary>
/// Workspace-wide display and default source configuration for an image-import type.
/// </summary>
public sealed class ImageImportTypeConfiguration : BaseEntity
{
    /// <summary>Stable image-import type key understood by the OCR pipeline.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Display name shown in settings and image-import selectors.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Optional default money source selected when this type is chosen.</summary>
    public Guid? SourceId { get; set; }

    public FinSource? Source { get; set; }
}
