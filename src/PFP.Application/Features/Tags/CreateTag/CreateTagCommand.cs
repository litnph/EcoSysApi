using MediatR;
using PFP.Application.Features.Tags.Common;

namespace PFP.Application.Features.Tags.CreateTag;

public sealed record CreateTagCommand(string Name, string Color) : IRequest<CreateTagResponse>;
