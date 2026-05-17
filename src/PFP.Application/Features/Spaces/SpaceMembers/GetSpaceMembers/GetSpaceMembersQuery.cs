using MediatR;

namespace PFP.Application.Features.Spaces.SpaceMembers.GetSpaceMembers;

public sealed record GetSpaceMembersQuery(Guid SpaceId) : IRequest<GetSpaceMembersResponse>;
