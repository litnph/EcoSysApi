using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Comments.AddComment;
using PFP.Application.Features.Comments.Common;
using PFP.Application.Features.Comments.DeleteComment;
using PFP.Application.Features.Comments.EditComment;
using PFP.Application.Features.Comments.GetComments;

namespace PFP.API.Controllers;

/// <summary>Cross-module threaded commentary (finance MVP anchored to transactions).</summary>
[ApiController]
[Authorize]
[Route("api/v1/comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public CommentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Threads every visible comment rooted on one entity anchor.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetCommentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetCommentsResponse>>> GetTree(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        [FromQuery(Name = "module_code")] string? moduleCode,
        CancellationToken cancellationToken)
    {
        var data = await _mediator.Send(
                new GetCommentsQuery(moduleCode ?? string.Empty, entityType, entityId),
                cancellationToken)
            .ConfigureAwait(false);

        return Ok(new ApiResponse<GetCommentsResponse> { Data = data });
    }

    /// <summary>Adds root or reply commentary.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AddCommentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AddCommentResponse>>> Create(
        [FromBody] AddCommentCommand command,
        CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<AddCommentResponse> { Data = created });
    }

    /// <summary>Allows the author to fix body text.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] EditCommentBody body,
        CancellationToken cancellationToken)
    {
        if (body is null)
            return BadRequest(new ApiResponse<object> { Success = false, Error = new { message = "Body is required." } });

        await _mediator.Send(new EditCommentCommand(id, body.Content), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }

    /// <summary>Soft-removes authored leaves or masks parents that still anchor replies.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCommentCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }
}

/// <summary>PUT body for commentary edits (<c>/api/v1/comments/{id}</c>).</summary>
public sealed class EditCommentBody
{
    /// <inheritdoc cref="EditCommentCommand.Content"/>
    public string Content { get; init; } = string.Empty;
}
