using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Authorization;
using PFP.API.Models;
using PFP.Application.Features.Translations.CreateTranslation;
using PFP.Application.Features.Translations.GetEntityTranslations;
using PFP.Application.Features.Translations.UpdateTranslation;

namespace PFP.API.Controllers;

/// <summary>Admin-only translation maintenance.</summary>
[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/translations")]
public sealed class TranslationsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public TranslationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a translation row.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateTranslationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateTranslationResponse>>> Create(
        [FromBody] CreateTranslationCommand body,
        CancellationToken cancellationToken)
    {
        var created = await _mediator.Send(body, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateTranslationResponse> { Data = created });
    }

    /// <summary>Updates an existing translation.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id,
        [FromBody] UpdateTranslationBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateTranslationCommand(id, body.Value), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { id } });
    }

    /// <summary>Lists every stored translation for one entity instance.</summary>
    [HttpGet("{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EntityTranslationAdminDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EntityTranslationAdminDto>>>> ListForEntity(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var rows = await _mediator
            .Send(new GetEntityTranslationsQuery(entityType, entityId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<IReadOnlyList<EntityTranslationAdminDto>> { Data = rows });
    }
}
