using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Sources.CreateSource;
using PFP.Application.Features.Sources.DeleteSource;
using PFP.Application.Features.Sources.GetSourceById;
using PFP.Application.Features.Sources.GetSources;
using PFP.Application.Features.Sources.UpdateSource;

namespace PFP.API.Controllers;

/// <summary>Finance sources CRUD (spec §5.3 — <c>/finance/sources</c>).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/sources")]
public sealed class SourcesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public SourcesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists sources for a finance space-module.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSourcesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSourcesResponse>>> List(
        [FromQuery(Name = "smodule_id")] Guid smoduleId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSourcesQuery(smoduleId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSourcesResponse> { Data = result });
    }

    /// <summary>Returns a single source with its current balance.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetSourceByIdResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSourceByIdResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSourceByIdQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSourceByIdResponse> { Data = result });
    }

    /// <summary>Creates a new finance source.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateSourceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateSourceResponse>>> Create(
        [FromBody] CreateSourceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateSourceResponse> { Data = result });
    }

    /// <summary>Updates an existing finance source.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateSourceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateSourceResponse>>> Update(
        Guid id,
        [FromBody] UpdateSourceBody body,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSourceCommand(
            id,
            body.Name,
            body.Type,
            body.CreditLimit,
            body.StatementDay,
            body.PaymentDueDay,
            body.MinInstallmentAmt,
            body.Currency,
            body.Icon,
            body.Color,
            body.SortOrder);

        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateSourceResponse> { Data = result });
    }

    /// <summary>Soft-deletes a finance source when it has no transactions.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteSourceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteSourceResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSourceCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteSourceResponse> { Data = result });
    }
}
