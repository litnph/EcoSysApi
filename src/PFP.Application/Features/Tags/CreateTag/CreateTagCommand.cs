using MediatR;
using PFP.Application.Features.Tags.Common;

namespace PFP.Application.Features.Tags.CreateTag;

public sealed record CreateTagCommand(Guid SmoduleId, string Name, string Color) : IRequest<CreateTagResponse>;
