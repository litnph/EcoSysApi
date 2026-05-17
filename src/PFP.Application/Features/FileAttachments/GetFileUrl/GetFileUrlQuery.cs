using MediatR;
using PFP.Application.Features.FileAttachments.Common;

namespace PFP.Application.Features.FileAttachments.GetFileUrl;

public sealed record GetFileUrlQuery(Guid AttachmentId) : IRequest<SignedFileUrlDto>;
