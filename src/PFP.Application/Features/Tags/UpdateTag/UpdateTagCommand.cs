using MediatR;
using PFP.Application.Features.Tags.Common;

namespace PFP.Application.Features.Tags.UpdateTag;

public sealed record UpdateTagCommand(Guid Id, string Name, string Color) : IRequest<UpdateTagResponse>;
