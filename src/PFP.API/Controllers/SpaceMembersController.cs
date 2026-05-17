using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Spaces.SpaceMembers.GetSpaceMembers;
using PFP.Application.Features.Spaces.SpaceMembers.InviteSpaceMember;
using PFP.Application.Features.Spaces.SpaceMembers.RemoveSpaceMember;
using PFP.Application.Features.Spaces.SpaceMembers.UpdateSpaceMemberRole;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Space membership invitations and role propagation (spec §4.6 — Sprint&nbsp;4 Task&nbsp;2).</summary>
[ApiController]
[Authorize]
[Route("api/v1/spaces/{spaceId:guid}/members")]
public sealed class SpaceMembersController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public SpaceMembersController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists active members (direct rows first).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSpaceMembersResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSpaceMembersResponse>>> List(
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSpaceMembersQuery(spaceId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSpaceMembersResponse> { Data = result });
    }

    /// <summary>Adds direct membership plus inherited copies for descendant spaces.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InviteSpaceMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<InviteSpaceMemberResponse>>> Invite(
        Guid spaceId,
        [FromBody] InviteMemberRequest? body,
        CancellationToken cancellationToken)
    {
        if (body is null)
        {
            return BadRequest(new ApiResponse<InviteSpaceMemberResponse>
            {
                Success = false,
                Error = new { message = "Request body is required." },
            });
        }

        var result = await _mediator.Send(
                new InviteSpaceMemberCommand(spaceId, body.UserId, body.Role),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(new ApiResponse<InviteSpaceMemberResponse> { Data = result });
    }

    /// <summary>Rewrites tier for the direct membership and propagated inherited tiers.</summary>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateSpaceMemberRoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateSpaceMemberRoleResponse>>> UpdateRole(
        Guid spaceId,
        Guid userId,
        [FromBody] UpdateRoleRequest body,
        CancellationToken cancellationToken)
    {
        if (body is null || body.Role is null)
            return BadRequest(new ApiResponse<UpdateSpaceMemberRoleResponse>
            {
                Success = false,
                Error = new { message = "Field 'role' is required." },
            });

        var propagate = body.PropagateToChildren ?? true;
        var newRole = body.Role.Value;

        var result = await _mediator.Send(
                new UpdateSpaceMemberRoleCommand(spaceId, userId, newRole, propagate),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(new ApiResponse<UpdateSpaceMemberRoleResponse> { Data = result });
    }

    /// <summary>Ends membership locally and cascades inherited removal downward.</summary>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RemoveSpaceMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RemoveSpaceMemberResponse>>> Remove(
        Guid spaceId,
        Guid userId,
        [FromQuery(Name = "remove_from_children")] bool removeFromChildren = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
                new RemoveSpaceMemberCommand(spaceId, userId, removeFromChildren),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(new ApiResponse<RemoveSpaceMemberResponse> { Data = result });
    }
}

/// <summary>POST body for invitations.</summary>
public sealed record InviteMemberRequest(Guid UserId, SpaceRole Role);

/// <summary>PUT body — <see cref="Role"/> mandatory for updates.</summary>
public sealed class UpdateRoleRequest
{
    /// <summary>New tier granted to both the direct membership and descendant inherited rows.</summary>
    public SpaceRole? Role { get; init; }

    /// <summary>Rewrites descendant inherited tiers (default true).</summary>
    public bool? PropagateToChildren { get; init; }
}
