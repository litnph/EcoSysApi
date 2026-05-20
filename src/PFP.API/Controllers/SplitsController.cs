using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Transactions.Splits.GetPendingSplits;
using PFP.Application.Features.Transactions.Splits.SettleSplit;

namespace PFP.API.Controllers;

/// <summary>Split reimbursement endpoints.</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/splits")]
public sealed class SplitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SplitsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists pending splits grouped by parent transaction.</summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<GetPendingSplitsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetPendingSplitsResponse>>> ListPending(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingSplitsQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetPendingSplitsResponse> { Data = result });
    }

    /// <summary>Settles one split by posting income to the selected source.</summary>
    [HttpPost("{id:guid}/settle")]
    [ProducesResponseType(typeof(ApiResponse<SettleSplitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SettleSplitResponse>>> Settle(
        Guid id,
        [FromBody] SettleSplitRequest body,
        CancellationToken cancellationToken)
    {
        var cmd = new SettleSplitCommand(id, body.PaymentSourceId, body.Amount);
        var result = await _mediator.Send(cmd, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<SettleSplitResponse> { Data = result });
    }
}
