namespace PFP.Application.Common.Interfaces;

/// <summary>S3-compatible object storage used for uploaded file payloads (Cloudflare R2).</summary>
public interface IStorageService
{
    /// <summary>Persists the bytes under <paramref name="key"/>; returns the same key for bookkeeping.</summary>
    Task<string> UploadAsync(Stream stream, string key, string mimeType, CancellationToken cancellationToken = default);

    /// <summary>Issues a time-limited HTTPS GET URL for an existing object.</summary>
    Task<string> GetSignedUrlAsync(string fileKey, TimeSpan expiry, CancellationToken cancellationToken = default);

    /// <summary>Removes the object; returns <c>false</c> when the key does not exist.</summary>
    Task<bool> DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
}
