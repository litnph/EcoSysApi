using MediatR;

namespace PFP.Application.Features.FileAttachments.DeleteFile;

public sealed record DeleteFileCommand(Guid Id) : IRequest<Unit>;
