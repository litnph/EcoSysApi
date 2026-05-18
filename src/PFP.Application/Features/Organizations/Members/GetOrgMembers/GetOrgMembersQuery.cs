using MediatR;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.Members.GetOrgMembers;

/// <summary>Lists every membership row (active + historical) of one organisation.</summary>
public sealed record GetOrgMembersQuery(Guid OrganizationId, bool IncludeInactive) : IRequest<GetOrgMembersResponse>;

/// <summary>Response wrapper.</summary>
public sealed record GetOrgMembersResponse(IReadOnlyList<OrgMemberDto> Members);
