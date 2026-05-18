using MediatR;
using PFP.Application.Features.Organizations.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.InviteOrgMember;

/// <summary>
/// Adds an existing platform user to the organisation. The full invitation flow with an email
/// link is owned by the Email module (out of scope of this command). Here we only persist the
/// <c>OrgMember</c> row; downstream space-membership cascading is done by the SpaceMembers feature.
/// </summary>
public sealed record InviteOrgMemberCommand(
    Guid OrganizationId,
    Guid UserId,
    OrgRole Role) : IRequest<InviteOrgMemberResponse>;

/// <summary>Wrapper around the persisted membership row.</summary>
public sealed record InviteOrgMemberResponse(OrgMemberDto Member);
