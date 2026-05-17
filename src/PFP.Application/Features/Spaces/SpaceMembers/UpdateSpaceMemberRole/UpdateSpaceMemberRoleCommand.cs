using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.UpdateSpaceMemberRole;

/// <summary>Changes a member's tier and optionally rewires inherited descendant rows.</summary>
public sealed record UpdateSpaceMemberRoleCommand(
    Guid SpaceId,
    Guid UserId,
    SpaceRole NewRole,
    bool PropagateToChildren = true) : IRequest<UpdateSpaceMemberRoleResponse>;
