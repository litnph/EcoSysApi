using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.DebtRecords.CreateDebtRecord;
using PFP.Application.Features.DebtRecords.DeleteDebtRecord;
using PFP.Application.Features.DebtRecords.GetDebtRecordDetail;
using PFP.Application.Features.DebtRecords.GetDebtRecords;
using PFP.Application.Features.DebtRecords.GetDebtSummary;
using PFP.Application.Features.DebtRecords.RecordDebtPayment;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Debt / loan records (read list, summary, detail; soft-delete mistaken entries).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/debt-records")]
public sealed class DebtRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DebtRecordsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists debt records for a finance module with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetDebtRecordsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetDebtRecordsResponse>>> List(
        [FromQuery(Name = "direction")] DebtDirection? direction,
        [FromQuery(Name = "status")] DebtStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDebtRecordsQuery(direction, status), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetDebtRecordsResponse> { Data = result });
    }

    /// <summary>Aggregated remaining balances and overdue counts.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<GetDebtSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetDebtSummaryResponse>>> Summary(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDebtSummaryQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetDebtSummaryResponse> { Data = result });
    }

    /// <summary>Returns one debt record with all ledger rows (oldest first).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetDebtRecordDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetDebtRecordDetailResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDebtRecordDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetDebtRecordDetailResponse> { Data = result });
    }

    /// <summary>Creates a manual debt record (no money movement; for back-filling external debts).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDebtRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateDebtRecordResponse>>> Create(
        [FromBody] CreateDebtRecordBody body,
        CancellationToken cancellationToken)
    {
        var command = new CreateDebtRecordCommand(
            body.Direction,
            body.PersonName,
            body.PersonContact,
            body.Amount,
            body.Currency,
            body.DueDate,
            body.Note);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateDebtRecordResponse> { Data = result });
    }

    /// <summary>Records a repayment against an existing debt record.</summary>
    /// <remarks>Internally dispatches the appropriate <c>debt_repay</c> / <c>loan_collect</c> transaction.</remarks>
    [HttpPost("{id:guid}/payment")]
    [ProducesResponseType(typeof(ApiResponse<RecordDebtPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RecordDebtPaymentResponse>>> RecordPayment(
        Guid id,
        [FromBody] RecordDebtPaymentBody body,
        CancellationToken cancellationToken)
    {
        var command = new RecordDebtPaymentCommand(id, body.SourceId, body.Amount, body.TxnDate, body.Note);
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<RecordDebtPaymentResponse> { Data = result });
    }

    /// <summary>Soft-deletes a mistaken debt record (only when no repayments / collections exist).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteDebtRecordResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteDebtRecordResponse>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteDebtRecordCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteDebtRecordResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="DebtRecordsController.Create"/>.</summary>
public sealed class CreateDebtRecordBody
{
    /// <summary><c>borrowed</c> (the user owes someone) or <c>lent</c> (someone owes the user).</summary>
    public DebtDirection Direction { get; init; }

    /// <summary>Counterparty name (max 200 chars).</summary>
    public string PersonName { get; init; } = string.Empty;

    /// <summary>Optional counterparty contact (phone, email, …).</summary>
    public string? PersonContact { get; init; }

    /// <summary>Original principal amount (positive magnitude).</summary>
    public long Amount { get; init; }

    /// <summary>Optional ISO-4217 currency override; defaults to <c>VND</c>.</summary>
    public string? Currency { get; init; }

    /// <summary>Optional repayment due date.</summary>
    public DateOnly? DueDate { get; init; }

    /// <summary>Optional free-form note (max 500 chars).</summary>
    public string? Note { get; init; }
}

/// <summary>JSON body for <see cref="DebtRecordsController.RecordPayment"/>.</summary>
public sealed class RecordDebtPaymentBody
{
    /// <summary>Finance source whose balance changes as a result of the payment.</summary>
    public Guid SourceId { get; init; }

    /// <summary>Payment magnitude (positive; cannot exceed remaining debt).</summary>
    public long Amount { get; init; }

    /// <summary>Business date of the payment.</summary>
    public DateOnly TxnDate { get; init; }

    /// <summary>Optional note (max 500 chars).</summary>
    public string? Note { get; init; }
}
