using MediatR;

namespace PFP.Application.Features.Spaces.SpaceMembers.RemoveSpaceMember;

/// <summary>Ends a user's membership directly on a node and cascades propagated removal.</summary>
public sealed record RemoveSpaceMemberCommand(
    Guid SpaceId,
    Guid UserId,
    bool RemoveFromChildren = true) : IRequest<RemoveSpaceMemberResponse>;
