using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.DebtRecords.DeleteDebtRecord;
using PFP.Application.Features.DebtRecords.GetDebtRecordDetail;
using PFP.Application.Features.DebtRecords.GetDebtRecords;
using PFP.Application.Features.DebtRecords.GetDebtSummary;
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
        [FromQuery(Name = "smodule_id")] Guid smoduleId,
        [FromQuery(Name = "direction")] DebtDirection? direction,
        [FromQuery(Name = "status")] DebtStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDebtRecordsQuery(smoduleId, direction, status), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetDebtRecordsResponse> { Data = result });
    }

    /// <summary>Aggregated remaining balances and overdue counts.</summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<GetDebtSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetDebtSummaryResponse>>> Summary(
        [FromQuery(Name = "smodule_id")] Guid smoduleId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDebtSummaryQuery(smoduleId), cancellationToken).ConfigureAwait(false);
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
