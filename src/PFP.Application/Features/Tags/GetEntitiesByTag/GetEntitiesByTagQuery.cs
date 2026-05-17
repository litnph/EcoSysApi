using MediatR;
using PFP.Application.Features.Tags.Common;

namespace PFP.Application.Features.Tags.GetEntitiesByTag;

public sealed record GetEntitiesByTagQuery(Guid TagId) : IRequest<GetEntitiesByTagResponse>;
