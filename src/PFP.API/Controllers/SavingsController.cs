using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Savings.CreateSaving;
using PFP.Application.Features.Savings.DeleteSaving;
using PFP.Application.Features.Savings.DepositToSaving;
using PFP.Application.Features.Savings.GetSavingDetail;
using PFP.Application.Features.Savings.GetSavings;
using PFP.Application.Features.Savings.UpdateSaving;
using PFP.Application.Features.Savings.WithdrawFromSaving;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Finance savings goals / books.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/savings")]
public sealed class SavingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetSavingsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSavingsResponse>>> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSavingsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSavingsResponse> { Data = result });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetSavingDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetSavingDetailResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSavingDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetSavingDetailResponse> { Data = result });
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateSavingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateSavingResponse>>> Create(
        [FromBody] CreateSavingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateSavingResponse> { Data = result });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateSavingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UpdateSavingResponse>>> Update(
        Guid id,
        [FromBody] UpdateSavingBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new UpdateSavingCommand(
            id,
            body.SourceId,
            body.Name,
            body.TargetAmount,
            body.InterestRate,
            body.StartDate,
            body.MaturityDate,
            body.Type,
            body.Status,
            body.Note);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<UpdateSavingResponse> { Data = result });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteSavingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteSavingResponse>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteSavingCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteSavingResponse> { Data = result });
    }

    [HttpPost("{id:guid}/deposit")]
    [ProducesResponseType(typeof(ApiResponse<DepositToSavingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DepositToSavingResponse>>> Deposit(
        Guid id,
        [FromBody] SavingCashMovementBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new DepositToSavingCommand(id, body.Amount, body.TxnDate, body.Note, body.MonthlyPeriodId);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DepositToSavingResponse> { Data = result });
    }

    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType(typeof(ApiResponse<WithdrawFromSavingResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WithdrawFromSavingResponse>>> Withdraw(
        Guid id,
        [FromBody] SavingCashMovementBody body,
        CancellationToken cancellationToken)
    {
        var cmd = new WithdrawFromSavingCommand(id, body.Amount, body.TxnDate, body.Note, body.MonthlyPeriodId);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<WithdrawFromSavingResponse> { Data = result });
    }
}

/// <summary>JSON body for PUT <c>/api/v1/finance/savings/{id}</c>.</summary>
public sealed record UpdateSavingBody(
    Guid SourceId,
    string Name,
    long? TargetAmount,
    decimal InterestRate,
    DateOnly StartDate,
    DateOnly? MaturityDate,
    SavingType Type,
    SavingStatus Status,
    string? Note);

/// <summary>JSON body for savings deposit / withdrawal.</summary>
public sealed record SavingCashMovementBody(
    long Amount,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId);
