namespace PFP.Application.Common.Interfaces;

/// <summary>Uploads GDPR JSON bundles to object storage and issues pre-signed GET URLs.</summary>
public interface IUserDataExportStorage
{
    /// <summary>Uploads UTF-8 JSON bytes to object storage.</summary>
    Task PutJsonObjectAsync(string objectKey, byte[] utf8Json, CancellationToken cancellationToken = default);

    /// <summary>Creates a time-limited HTTPS URL for downloading the object.</summary>
    string CreatePresignedGetUrl(string objectKey, TimeSpan lifetime);
}
