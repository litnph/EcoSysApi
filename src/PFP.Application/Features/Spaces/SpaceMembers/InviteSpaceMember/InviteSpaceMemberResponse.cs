using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.InviteSpaceMember;

/// <summary>Summary of the propagated invite mutation.</summary>
public sealed record InviteSpaceMemberResponse(
    Guid SpaceId,
    Guid UserId,
    SpaceRole Role,
    int DescendantInheritedRowsCreated);
