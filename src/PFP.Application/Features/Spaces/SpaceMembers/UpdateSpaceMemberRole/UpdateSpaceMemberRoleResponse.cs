namespace PFP.Application.Features.Spaces.SpaceMembers.UpdateSpaceMemberRole;

public sealed record UpdateSpaceMemberRoleResponse(Guid SpaceId, Guid UserId, int DescendantInheritedRowsUpdated);
