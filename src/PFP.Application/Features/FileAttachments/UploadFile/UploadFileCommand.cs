using MediatR;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.UploadFile;

public sealed record UploadFileCommand(
    string EntityType,
    Guid EntityId,
    string FileName,
    string MimeType,
    Stream FileContent,
    long DeclaredContentLength,
    int MaxFileSizeMb,
    bool IsPublic) : IRequest<FileAttachmentDto>;
