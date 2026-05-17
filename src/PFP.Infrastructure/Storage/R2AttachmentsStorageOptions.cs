namespace PFP.Infrastructure.Storage;

/// <summary>Cloudflare R2 (S3-compatible) bucket used for user-uploaded file attachments.</summary>
public sealed class R2AttachmentsStorageOptions
{
    /// <summary>Configuration section (<c>R2:Attachments</c>).</summary>
    public const string SectionName = "R2:Attachments";

    /// <summary>R2 account / API token id (access key).</summary>
    public string AccessKeyId { get; set; } = string.Empty;

    /// <summary>R2 secret.</summary>
    public string SecretAccessKey { get; set; } = string.Empty;

    /// <summary>Bucket name.</summary>
    public string Bucket { get; set; } = string.Empty;

    /// <summary>Account id subdomain (<c>{id}.r2.cloudflarestorage.com</c>).</summary>
    public string AccountId { get; set; } = string.Empty;
}
