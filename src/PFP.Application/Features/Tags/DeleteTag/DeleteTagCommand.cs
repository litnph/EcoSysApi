using MediatR;

namespace PFP.Application.Features.Tags.DeleteTag;

public sealed record DeleteTagCommand(Guid Id) : IRequest<Unit>;
