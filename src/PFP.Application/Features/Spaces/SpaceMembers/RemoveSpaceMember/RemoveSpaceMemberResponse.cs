namespace PFP.Application.Features.Spaces.SpaceMembers.RemoveSpaceMember;

public sealed record RemoveSpaceMemberResponse(Guid SpaceId, Guid UserId, int DescendantInheritedRowsRemoved);
