using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Tags.AddTagToEntity;
using PFP.Application.Features.Tags.Common;
using PFP.Application.Features.Tags.CreateTag;
using PFP.Application.Features.Tags.DeleteTag;
using PFP.Application.Features.Tags.GetEntitiesByTag;
using PFP.Application.Features.Tags.GetTags;
using PFP.Application.Features.Tags.RemoveTagFromEntity;
using PFP.Application.Features.Tags.UpdateTag;

namespace PFP.API.Controllers;

/// <summary>Finance taxonomy — tags on space modules plus polymorphic attaches.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/tags")]
public sealed class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public TagsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns every tag scoped to one finance module instance.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TagListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TagListItemDto>>>> ListByModule(
        CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetTagsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<IReadOnlyList<TagListItemDto>> { Data = rows });
    }

    /// <summary>Registers a new tag.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateTagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateTagResponse>>> Create(
        [FromBody] CreateTagCommand command,
        CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateTagResponse> { Data = created });
    }

    /// <summary>Patches label / colour metadata.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateTagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateTagResponse>>> Update(
        Guid id,
        [FromBody] UpdateTagBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateTagCommand(id, body.Name, body.Color), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateTagResponse> { Data = result });
    }

    /// <summary>Deletes an unused (<c>usage_count == 0</c>) definition.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTagCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }

    /// <summary>Assigns tag <paramref name="id"/> to another aggregate.</summary>
    [HttpPost("{id:guid}/attach")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Attach(Guid id, [FromBody] AttachTagRequest body, CancellationToken cancellationToken)
    {
        if (body is null)
            return BadRequest(new ApiResponse<object> { Success = false, Error = new { message = "Body is required." } });

        await _mediator.Send(new AddTagToEntityCommand(id, body.EntityType, body.EntityId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { tagId = id, body.EntityType, body.EntityId } });
    }

    /// <summary>Unassigns a tag combination from an anchored entity row.</summary>
    [HttpDelete("{id:guid}/detach")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Detach(
        Guid id,
        [FromQuery(Name = "entity_type")] string entityType,
        [FromQuery(Name = "entity_id")] Guid entityId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveTagFromEntityCommand(id, entityType, entityId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { tagId = id, entityType, entityId } });
    }

    /// <summary>Enumerates aggregates currently linked through this tag id.</summary>
    [HttpGet("{id:guid}/entities")]
    [ProducesResponseType(typeof(ApiResponse<GetEntitiesByTagResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetEntitiesByTagResponse>>> EntitiesByTag(
        Guid id,
        CancellationToken cancellationToken)
    {
        var data = await _mediator.Send(new GetEntitiesByTagQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetEntitiesByTagResponse> { Data = data });
    }
}

/// <summary>Rename / recolour finance tag catalog entries.</summary>
public sealed class UpdateTagBody
{
    /// <summary>Display title unique per finance module row.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary><c>#RRGGBB</c> hue.</summary>
    public string Color { get; init; } = string.Empty;
}

/// <summary>Payload for attaching a tag to finance anchors.</summary>
public sealed record AttachTagRequest(string EntityType, Guid EntityId);
