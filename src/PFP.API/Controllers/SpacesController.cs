using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Spaces.CreateSpace;
using PFP.Application.Features.Spaces.GetSpaceTree;
using PFP.Application.Features.Spaces.MoveSpace;

namespace PFP.API.Controllers;

/// <summary>Organisation workspace tree APIs (spec §4.5 — Sprint&nbsp;4).</summary>
[ApiController]
[Authorize]
[Route("api/v1/spaces")]
public sealed class SpacesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public SpacesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a new space beneath an optional parent (materialised-path + cascade).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateSpaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateSpaceResponse>>> Create(
        [FromBody] CreateSpaceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateSpaceResponse> { Data = result });
    }

    /// <summary>Returns the full nested workspace tree with finance-module hints.</summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(ApiResponse<GetSpaceTreeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSpaceTreeResponse>>> GetTree(
        [FromQuery(Name = "org_id")] Guid orgId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSpaceTreeQuery(orgId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSpaceTreeResponse> { Data = result });
    }

    /// <summary>Re-parents a space and rewires subtree paths atomically.</summary>
    [HttpPost("{id:guid}/move")]
    [ProducesResponseType(typeof(ApiResponse<MoveSpaceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MoveSpaceResponse>>> Move(
        Guid id,
        [FromBody] MoveSpaceRequestBody body,
        CancellationToken cancellationToken)
    {
        var command = new MoveSpaceCommand(id, body.NewParentId);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<MoveSpaceResponse> { Data = result });
    }
}

/// <summary>REST body for POST <c>/api/v1/spaces/{id}/move</c>.</summary>
public sealed class MoveSpaceRequestBody
{
    /// <summary>Destination parent workspace; <c>null</c> re-homes directly under organisation root semantics.</summary>
    public Guid? NewParentId { get; init; }
}
