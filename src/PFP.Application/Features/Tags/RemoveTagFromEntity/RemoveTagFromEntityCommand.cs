using MediatR;

namespace PFP.Application.Features.Tags.RemoveTagFromEntity;

public sealed record RemoveTagFromEntityCommand(Guid TagId, string EntityType, Guid EntityId) : IRequest<Unit>;
