using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.GetSpaceMembers;

public sealed record GetSpaceMembersResponse(IReadOnlyList<SpaceMemberListDto> Items);

/// <summary>Active membership row surfaced to the owning space admins.</summary>
public sealed record SpaceMemberListDto(
    Guid UserId,
    string Email,
    string FullName,
    SpaceRole Role,
    bool Inherited,
    Guid? InheritedFromSpaceId,
    Guid? InvitedBy,
    DateTime? JoinedAt);
