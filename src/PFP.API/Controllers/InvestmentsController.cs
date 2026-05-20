using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Investments.CreateInvestment;
using PFP.Application.Features.Investments.DeleteInvestment;
using PFP.Application.Features.Investments.GetInvestmentDetail;
using PFP.Application.Features.Investments.GetInvestments;
using PFP.Application.Features.Investments.RecordInvestmentTxn;
using PFP.Application.Features.Investments.UpdateInvestment;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Finance investments and ledger.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/investments")]
public sealed class InvestmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvestmentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetInvestmentsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetInvestmentsResponse>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvestmentsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetInvestmentsResponse> { Data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetInvestmentDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetInvestmentDetailResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInvestmentDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetInvestmentDetailResponse> { Data = result });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateInvestmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateInvestmentResponse>>> Create(
        [FromBody] CreateInvestmentCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateInvestmentResponse> { Data = result });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateInvestmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateInvestmentResponse>>> Update(
        Guid id,
        [FromBody] UpdateInvestmentBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new UpdateInvestmentCommand(
            id,
            body.Name,
            body.Type,
            body.CurrentValue,
            body.Currency,
            body.Note);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateInvestmentResponse> { Data = result });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteInvestmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteInvestmentResponse>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteInvestmentCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteInvestmentResponse> { Data = result });
    }

    [HttpPost("{id:guid}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<RecordInvestmentTxnResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RecordInvestmentTxnResponse>>> RecordTxn(
        Guid id,
        [FromBody] RecordInvestmentTxnBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new RecordInvestmentTxnCommand(
            id,
            body.TxnType,
            body.Amount,
            body.Quantity,
            body.PricePerUnit,
            body.TxnDate,
            body.Note,
            body.LinkedTxnId);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<RecordInvestmentTxnResponse> { Data = result });
    }
}

/// <summary>JSON body for PUT <c>/api/v1/finance/investments/{id}</c>.</summary>
public sealed record UpdateInvestmentBody(
    string Name,
    InvestmentType Type,
    decimal CurrentValue,
    string Currency,
    string? Note);

/// <summary>JSON body for POST <c>/api/v1/finance/investments/{id}/transactions</c>.</summary>
public sealed record RecordInvestmentTxnBody(
    InvestmentTxnType TxnType,
    long Amount,
    decimal? Quantity,
    decimal? PricePerUnit,
    DateOnly TxnDate,
    string? Note,
    Guid? LinkedTxnId);
