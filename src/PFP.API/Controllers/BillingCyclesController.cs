using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;
using PFP.Application.Features.BillingCycles.GetBillingCycleDetail;
using PFP.Application.Features.BillingCycles.GetBillingCycles;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Credit-card billing cycles (list, detail, generate, close, pay).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/billing-cycles")]
public sealed class BillingCyclesController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public BillingCyclesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists billing cycles for a finance module.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetBillingCyclesResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetBillingCyclesResponse>>> List(
        [FromQuery(Name = "source_id")] Guid? sourceId,
        [FromQuery(Name = "status")] BillingCycleStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBillingCyclesQuery(sourceId, status), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetBillingCyclesResponse> { Data = result });
    }

    /// <summary>Returns billing cycle detail with transactions (newest first).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetBillingCycleDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetBillingCycleDetailResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBillingCycleDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetBillingCycleDetailResponse> { Data = result });
    }

    /// <summary>Generates a new open billing cycle for a credit-card source.</summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<GenerateBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GenerateBillingCycleResponse>>> Generate(
        [FromBody] GenerateBillingCycleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GenerateBillingCycleResponse> { Data = result });
    }

    /// <summary>Closes an open billing cycle and reconciles totals.</summary>
    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<CloseBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CloseBillingCycleResponse>>> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CloseBillingCycleCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CloseBillingCycleResponse> { Data = result });
    }

    /// <summary>Posts a payment toward a closed or overdue cycle.</summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(ApiResponse<PayBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PayBillingCycleResponse>>> Pay(
        Guid id,
        [FromBody] PayBillingCycleBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PayBillingCycleCommand(id, body.PaymentSourceId, body.Amount),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<PayBillingCycleResponse> { Data = result });
    }
}

/// <summary>JSON body for <see cref="BillingCyclesController.Pay"/>.</summary>
public sealed class PayBillingCycleBody
{
    /// <summary>Source that funds the payment (bank account, cash, …).</summary>
    public Guid PaymentSourceId { get; set; }

    /// <summary>Payment amount (must not exceed remaining statement balance).</summary>
    public long Amount { get; set; }
}
