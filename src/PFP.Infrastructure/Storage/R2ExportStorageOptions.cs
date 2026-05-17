namespace PFP.Infrastructure.Storage;

/// <summary>Cloudflare R2 (S3-compatible) settings for GDPR exports.</summary>
public sealed class R2ExportStorageOptions
{
    /// <summary>Configuration section (<c>R2:Export</c>).</summary>
    public const string SectionName = "R2:Export";

    /// <summary>R2 account / API token id (access key).</summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>R2 secret.</summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>Bucket name.</summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>Account id subdomain, e.g. <c>abc123</c> for <c>https://abc123.r2.cloudflarestorage.com</c>.</summary>
    public string AccountId { get; set; } = string.Empty;
}
