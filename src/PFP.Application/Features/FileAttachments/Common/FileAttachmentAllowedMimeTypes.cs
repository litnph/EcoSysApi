namespace PFP.Application.Features.FileAttachments.Common;

/// <summary>Allowed MIME values for uploads (whitelist).</summary>
public static class FileAttachmentAllowedMimeTypes
{
    /// <summary>Exact Content-Type strings accepted at upload.</summary>
    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    };

    /// <summary>Maps whitelist MIME → safe file suffix for stored object keys.</summary>
    public static string ToFileExtension(string mimeType)
    {
        if (!All.Contains(mimeType))
            throw new ArgumentOutOfRangeException(nameof(mimeType), mimeType, "MIME type must be whitelisted.");

        return mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            "application/pdf" => "pdf",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
            _ => "bin",
        };
    }

    /// <summary>Returns a sanitised safe display filename (basename, max length).</summary>
    public static string SanitizeOriginalFileName(string? originalFileName)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            return "file";

        var name = Path.GetFileName(originalFileName.Trim());
        if (string.IsNullOrEmpty(name))
            return "file";

        const int maxLen = 255;
        return name.Length <= maxLen ? name : name[..maxLen];
    }
}
