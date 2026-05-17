namespace PFP.Application.Common.Options;

/// <summary>Defaults for download links returned by file-attachment flows.</summary>
public sealed class FileStorageOptions
{
    /// <summary>Configuration section (<c>FileStorage</c>).</summary>
    public const string SectionName = "FileStorage";

    /// <summary>Default presigned URL lifetime (minutes) when clients do not override.</summary>
    public int DefaultSignedUrlMinutes { get; set; } = 15;
}
