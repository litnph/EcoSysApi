using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Storage;

/// <summary>AWS S3 SDK client pointed at Cloudflare R2 for general-purpose file attachments.</summary>
public sealed class CloudflareR2StorageService : IStorageService, IDisposable
{
    private readonly R2AttachmentsStorageOptions _opt;
    private AmazonS3Client? _client;

    /// <summary>Creates the helper.</summary>
    public CloudflareR2StorageService(IOptions<R2AttachmentsStorageOptions> options) => _opt = options.Value;

    /// <inheritdoc/>
    public async Task<string> UploadAsync(Stream stream, string key, string mimeType, CancellationToken cancellationToken = default)
    {
        var put = new PutObjectRequest
        {
            BucketName = RequireOptions().Bucket,
            Key = key,
            InputStream = stream,
            ContentType = mimeType,
        };

        await GetClient().PutObjectAsync(put, cancellationToken).ConfigureAwait(false);
        return key;
    }

    /// <inheritdoc/>
    public Task<string> GetSignedUrlAsync(string fileKey, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var url = GetClient().GetPreSignedURL(
            new GetPreSignedUrlRequest
            {
                BucketName = RequireOptions().Bucket,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry),
            });
        return Task.FromResult(url);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        await GetClient()
            .DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = RequireOptions().Bucket,
                    Key = fileKey,
                },
                cancellationToken)
            .ConfigureAwait(false);

        // S3 / R2 do not reliably signal "missing key" separately from success — treat DELETE as acknowledged.
        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
    }

    private AmazonS3Client GetClient() =>
        _client ??= new AmazonS3Client(
            RequireOptions().AccessKeyId,
            RequireOptions().SecretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = $"https://{RequireOptions().AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                AuthenticationRegion = "auto",
            });

    private R2AttachmentsStorageOptions RequireOptions()
    {
        if (string.IsNullOrWhiteSpace(_opt.AccountId)
            || string.IsNullOrWhiteSpace(_opt.Bucket)
            || string.IsNullOrWhiteSpace(_opt.AccessKeyId)
            || string.IsNullOrWhiteSpace(_opt.SecretAccessKey))
        {
            throw new InvalidOperationException(
                "R2:Attachments requires AccountId, Bucket, AccessKeyId, and SecretAccessKey.");
        }

        return _opt;
    }
}
