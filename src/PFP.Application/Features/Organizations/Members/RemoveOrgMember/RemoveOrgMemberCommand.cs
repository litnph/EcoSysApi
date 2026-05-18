using MediatR;

namespace PFP.Application.Features.Organizations.Members.RemoveOrgMember;

/// <summary>Removes a member from the organisation (marks the row inactive + revokes space memberships).</summary>
public sealed record RemoveOrgMemberCommand(Guid OrganizationId, Guid MemberId) : IRequest<RemoveOrgMemberResponse>;

/// <summary>Response acknowledging the removal.</summary>
public sealed record RemoveOrgMemberResponse(Guid MemberId);
