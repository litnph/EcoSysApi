using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Categories.CreateCategory;
using PFP.Application.Features.Categories.DeleteCategory;
using PFP.Application.Features.Categories.GetCategories;
using PFP.Application.Features.Categories.GetFlatCategories;
using PFP.Application.Features.Categories.UpdateCategory;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Finance categories — tree + flat list + CRUD.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public CategoriesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns nested categories for a module and kind.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetCategoriesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetCategoriesResponse>>> GetTree(
        [FromQuery(Name = "smodule_id")] Guid smoduleId,
        [FromQuery(Name = "kind")] CategoryKind kind,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(smoduleId, kind), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetCategoriesResponse> { Data = result });
    }

    /// <summary>Returns a flat category list for dropdowns.</summary>
    [HttpGet("flat")]
    [ProducesResponseType(typeof(ApiResponse<GetFlatCategoriesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetFlatCategoriesResponse>>> GetFlat(
        [FromQuery(Name = "smodule_id")] Guid smoduleId,
        [FromQuery(Name = "kind")] CategoryKind kind,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFlatCategoriesQuery(smoduleId, kind), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetFlatCategoriesResponse> { Data = result });
    }

    /// <summary>Creates a category.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateCategoryResponse>>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateCategoryResponse> { Data = result });
    }

    /// <summary>Updates a category.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateCategoryResponse>>> Update(
        Guid id,
        [FromBody] UpdateCategoryBody body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(
            id,
            body.Name,
            body.Kind,
            body.ParentId,
            body.Icon,
            body.Color,
            body.SortOrder,
            body.IsDefault);

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateCategoryResponse> { Data = result });
    }

    /// <summary>Soft-deletes a category.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteCategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteCategoryResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteCategoryResponse> { Data = result });
    }
}
