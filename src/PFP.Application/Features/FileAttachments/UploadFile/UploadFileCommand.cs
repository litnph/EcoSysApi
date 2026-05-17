using MediatR;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.UploadFile;

/// <summary>Uploaded file packaged for MediatR (controller reads multipart form).</summary>
public sealed record UploadFileCommand(
    string ModuleCode,
    string EntityType,
    Guid EntityId,
    string FileName,
    string MimeType,
    Stream FileContent,
    long DeclaredContentLength,
    int MaxFileSizeMb,
    bool IsPublic) : IRequest<FileAttachmentDto>;
