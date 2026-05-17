namespace PFP.Application.Features.FileAttachments.Common;

/// <summary>Persisted attachment row plus fresh presigned download link.</summary>
public sealed record FileAttachmentDto(
    Guid Id,
    string ModuleCode,
    string EntityType,
    Guid EntityId,
    string FileName,
    string MimeType,
    long FileSize,
    bool IsPublic,
    DateTime CreatedAtUtc,
    string SignedUrl);

/// <summary>Short-lived signed GET URL envelope.</summary>
public sealed record SignedFileUrlDto(string Url, DateTime ExpiresAtUtc);

/// <summary>List projection without signing (call <c>/url</c> per item when needed).</summary>
public sealed record FileAttachmentSummaryDto(
    Guid Id,
    string FileName,
    string MimeType,
    long FileSize,
    bool IsPublic,
    DateTime UploadedAtUtc,
    Guid UploadedBy);

/// <summary>Transaction attachment list envelope.</summary>
public sealed record GetTransactionAttachmentsResponse(IReadOnlyList<FileAttachmentSummaryDto> Items);
