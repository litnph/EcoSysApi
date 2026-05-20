using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Members.Common;
using PFP.Application.Features.Members.CreateMember;
using PFP.Application.Features.Members.DeleteMember;
using PFP.Application.Features.Members.GetMembers;
using PFP.Application.Features.Members.UpdateMember;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Admin-only member management.</summary>
[ApiController]
[Authorize]
[Route("api/v1/members")]
public sealed class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetMembersResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetMembersResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMembersQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateMemberResponse>>> Create(
        [FromBody] CreateMemberBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
                new CreateMemberCommand(body.Email, body.Password, body.FullName, body.Role),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    [HttpPut("{memberId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateMemberResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateMemberResponse>>> Update(
        Guid memberId,
        [FromBody] UpdateMemberBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
                new UpdateMemberCommand(memberId, body.FullName, body.Role, body.IsActive, body.NewPassword),
                cancellationToken)
            .ConfigureAwait(false);
        return Ok(ApiResults.Ok(result));
    }

    [HttpDelete("{memberId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid memberId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMemberCommand(memberId), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}

public sealed class CreateMemberBody
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
}

public sealed class UpdateMemberBody
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public bool IsActive { get; set; } = true;
    public string? NewPassword { get; set; }
}
