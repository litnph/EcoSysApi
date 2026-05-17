using MediatR;

namespace PFP.Application.Features.Tags.AddTagToEntity;

public sealed record AddTagToEntityCommand(Guid TagId, string EntityType, Guid EntityId) : IRequest<Unit>;
