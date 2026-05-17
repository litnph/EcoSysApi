using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Storage;

/// <summary>R2-backed storage — lazily creates the S3 client when credentials are present.</summary>
public sealed class R2UserDataExportStorage : IUserDataExportStorage, IDisposable
{
    private readonly R2ExportStorageOptions _opt;
    private AmazonS3Client? _client;

    /// <summary>Creates the storage helper.</summary>
    public R2UserDataExportStorage(IOptions<R2ExportStorageOptions> options) => _opt = options.Value;

    /// <inheritdoc/>
    public async Task PutJsonObjectAsync(string objectKey, byte[] utf8Json, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream(utf8Json);
        var put = new PutObjectRequest
        {
            BucketName = RequireOptions().Bucket,
            Key = objectKey,
            InputStream = ms,
            ContentType = "application/json; charset=utf-8",
        };
        await GetClient().PutObjectAsync(put, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public string CreatePresignedGetUrl(string objectKey, TimeSpan lifetime) =>
        GetClient().GetPreSignedURL(
            new GetPreSignedUrlRequest
            {
                BucketName = RequireOptions().Bucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(lifetime),
            });

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

    private R2ExportStorageOptions RequireOptions()
    {
        if (string.IsNullOrWhiteSpace(_opt.AccountId)
            || string.IsNullOrWhiteSpace(_opt.Bucket)
            || string.IsNullOrWhiteSpace(_opt.AccessKeyId)
            || string.IsNullOrWhiteSpace(_opt.SecretAccessKey))
        {
            throw new InvalidOperationException(
                "R2:Export must set AccountId, Bucket, AccessKeyId, and SecretAccessKey to process GDPR exports.");
        }

        return _opt;
    }
}
