using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Sources.CreateSource;
using PFP.Application.Features.Sources.DeleteSource;
using PFP.Application.Features.Sources.GetSourceById;
using PFP.Application.Features.Sources.GetSources;
using PFP.Application.Features.Sources.ApplySourcesRecalculate;
using PFP.Application.Features.Sources.CreateBalanceAdjustment;
using PFP.Application.Features.Sources.GetSourceBalanceLedger;
using PFP.Application.Features.Sources.GetSourcesRecalculatePreview;
using PFP.Application.Features.Sources.RecalculateSourceBalance;
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
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSourcesQuery(), cancellationToken).ConfigureAwait(false);
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

    /// <summary>Running balance ledger for an asset source (ordered by transaction date).</summary>
    [HttpGet("{id:guid}/balance-ledger")]
    [ProducesResponseType(typeof(ApiResponse<GetSourceBalanceLedgerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSourceBalanceLedgerResponse>>> BalanceLedger(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new GetSourceBalanceLedgerQuery(id), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<GetSourceBalanceLedgerResponse> { Data = result });
    }

    /// <summary>Posts a signed balance adjustment (excluded from monthly reports).</summary>
    [HttpPost("{id:guid}/balance-adjustments")]
    [ProducesResponseType(typeof(ApiResponse<CreateBalanceAdjustmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateBalanceAdjustmentResponse>>> CreateBalanceAdjustment(
        Guid id,
        [FromBody] CreateBalanceAdjustmentBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateBalanceAdjustmentCommand(id, body.Amount, body.TxnDate, body.Note),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateBalanceAdjustmentResponse> { Data = result });
    }

    /// <summary>Preview stored vs computed balance (and credit utilization) for every source.</summary>
    [HttpGet("recalculate-preview")]
    [ProducesResponseType(typeof(ApiResponse<GetSourcesRecalculatePreviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSourcesRecalculatePreviewResponse>>> RecalculatePreview(
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new GetSourcesRecalculatePreviewQuery(), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<GetSourcesRecalculatePreviewResponse> { Data = result });
    }

    /// <summary>Applies recomputed balances for the selected sources after user confirmation.</summary>
    [HttpPost("recalculate-balance/apply")]
    [ProducesResponseType(typeof(ApiResponse<ApplySourcesRecalculateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ApplySourcesRecalculateResponse>>> ApplyRecalculate(
        [FromBody] ApplySourcesRecalculateBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new ApplySourcesRecalculateCommand(body.SourceIds), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<ApplySourcesRecalculateResponse> { Data = result });
    }

    /// <summary>Recomputes stored balance from opening balance and transaction legs.</summary>
    [HttpPost("{id:guid}/recalculate-balance")]
    [ProducesResponseType(typeof(ApiResponse<RecalculateSourceBalanceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RecalculateSourceBalanceResponse>>> RecalculateBalance(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new RecalculateSourceBalanceCommand(id), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<RecalculateSourceBalanceResponse> { Data = result });
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

/// <summary>JSON body for bulk balance recalculate after preview confirmation.</summary>
public sealed class ApplySourcesRecalculateBody
{
    public List<Guid> SourceIds { get; set; } = [];
}
