using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.BillingCycles.Commands.AddBillingCycleItem;
using PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.DeleteBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;
using PFP.Application.Features.BillingCycles.Commands.RemoveBillingCycleItem;
using PFP.Application.Features.BillingCycles.Commands.UpdateBillingCycleReconciliation;
using PFP.Application.Features.BillingCycles.GetBillingCycleAddableTransactions;
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

    /// <summary>Lists deferred transactions eligible to be added to an open billing cycle.</summary>
    [HttpGet("{id:guid}/addable-transactions")]
    [ProducesResponseType(typeof(ApiResponse<GetBillingCycleAddableTransactionsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetBillingCycleAddableTransactionsResponse>>> GetAddableTransactions(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetBillingCycleAddableTransactionsQuery(id),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetBillingCycleAddableTransactionsResponse> { Data = result });
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

    /// <summary>Maps eligible deferred transactions into an open cycle and recalculates totals.</summary>
    [HttpPost("{id:guid}/refresh")]
    [ProducesResponseType(typeof(ApiResponse<RefreshBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefreshBillingCycleResponse>>> Refresh(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshBillingCycleCommand(id), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<RefreshBillingCycleResponse> { Data = result });
    }

    /// <summary>Adds a deferred transaction to an open statement.</summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<RefreshBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefreshBillingCycleResponse>>> AddItem(
        Guid id,
        [FromBody] BillingCycleItemBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddBillingCycleItemCommand(id, body.TransactionId),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<RefreshBillingCycleResponse> { Data = result });
    }

    /// <summary>Removes a transaction from an open statement (soft line remove).</summary>
    [HttpDelete("{id:guid}/items/{transactionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RefreshBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RefreshBillingCycleResponse>>> RemoveItem(
        Guid id,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveBillingCycleItemCommand(id, transactionId),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<RefreshBillingCycleResponse> { Data = result });
    }

    /// <summary>Updates reconciliation note / issuer statement amount.</summary>
    [HttpPatch("{id:guid}/reconciliation")]
    [ProducesResponseType(typeof(ApiResponse<GenerateBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GenerateBillingCycleResponse>>> UpdateReconciliation(
        Guid id,
        [FromBody] BillingCycleReconciliationBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateBillingCycleReconciliationCommand(
                id,
                body.ReconciliationNote,
                body.IssuerStatementAmount),
            cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GenerateBillingCycleResponse> { Data = result });
    }

    /// <summary>Deletes an open billing cycle and its statement lines.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GenerateBillingCycleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GenerateBillingCycleResponse>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteBillingCycleCommand(id), cancellationToken)
            .ConfigureAwait(false);
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

/// <summary>JSON body for add-item on an open billing cycle.</summary>
public sealed class BillingCycleItemBody
{
    public Guid TransactionId { get; set; }
}

/// <summary>JSON body for reconciliation fields on a billing cycle.</summary>
public sealed class BillingCycleReconciliationBody
{
    public string? ReconciliationNote { get; set; }

    public long? IssuerStatementAmount { get; set; }
}
