using MediatR;
using PFP.Application.Features.Organizations.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.UpdateOrgMemberRole;

/// <summary>Changes the org-level role of a single member. Owner transfer is out of scope here.</summary>
public sealed record UpdateOrgMemberRoleCommand(
    Guid OrganizationId,
    Guid MemberId,
    OrgRole Role) : IRequest<UpdateOrgMemberRoleResponse>;

/// <summary>Response wrapper.</summary>
public sealed record UpdateOrgMemberRoleResponse(OrgMemberDto Member);
