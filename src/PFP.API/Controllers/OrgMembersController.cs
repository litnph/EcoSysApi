using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Organizations.Members.GetOrgMembers;
using PFP.Application.Features.Organizations.Members.InviteOrgMember;
using PFP.Application.Features.Organizations.Members.RemoveOrgMember;
using PFP.Application.Features.Organizations.Members.UpdateOrgMemberRole;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Manages members of one organisation (list / invite / update role / remove).</summary>
[ApiController]
[Authorize]
[Route("api/v1/organizations/{orgId:guid}/members")]
public sealed class OrgMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public OrgMembersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists active members; pass <c>includeInactive=true</c> to retrieve historical rows as well.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetOrgMembersResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetOrgMembersResponse>>> List(
        Guid orgId,
        [FromQuery(Name = "include_inactive")] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrgMembersQuery(orgId, includeInactive), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetOrgMembersResponse> { Data = result });
    }

    /// <summary>Adds an existing platform user to the organisation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InviteOrgMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InviteOrgMemberResponse>>> Invite(
        Guid orgId,
        [FromBody] InviteOrgMemberBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new InviteOrgMemberCommand(orgId, body.UserId, body.Role), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<InviteOrgMemberResponse> { Data = result });
    }

    /// <summary>Changes a member's role inside the organisation.</summary>
    [HttpPut("{memberId:guid}/role")]
    [ProducesResponseType(typeof(ApiResponse<UpdateOrgMemberRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateOrgMemberRoleResponse>>> UpdateRole(
        Guid orgId,
        Guid memberId,
        [FromBody] UpdateOrgMemberRoleBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new UpdateOrgMemberRoleCommand(orgId, memberId, body.Role), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateOrgMemberRoleResponse> { Data = result });
    }

    /// <summary>Removes a member from the organisation.</summary>
    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RemoveOrgMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RemoveOrgMemberResponse>>> Remove(
        Guid orgId,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new RemoveOrgMemberCommand(orgId, memberId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<RemoveOrgMemberResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="OrgMembersController.Invite"/>.</summary>
public sealed class InviteOrgMemberBody
{
    /// <summary>Identifier of the invitee (must already have a platform account).</summary>
    public Guid UserId { get; init; }

    /// <summary>Role to grant: <c>Member</c> or <c>Admin</c> (Owner transfer is a separate flow).</summary>
    public OrgRole Role { get; init; } = OrgRole.Member;
}

/// <summary>JSON body for <see cref="OrgMembersController.UpdateRole"/>.</summary>
public sealed class UpdateOrgMemberRoleBody
{
    /// <summary>New role for the member.</summary>
    public OrgRole Role { get; init; }
}
