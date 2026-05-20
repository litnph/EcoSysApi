using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.InstallmentPlans.Commands.CancelInstallmentPlan;
using PFP.Application.Features.InstallmentPlans.Commands.CreateInstallmentPlan;
using PFP.Application.Features.InstallmentPlans.Commands.RecordInstallmentPayment;
using PFP.Application.Features.InstallmentPlans.GetInstallmentPlanDetail;
using PFP.Application.Features.InstallmentPlans.GetInstallmentPlans;
using PFP.Domain.Enums;

namespace PFP.API.Controllers;

/// <summary>Installment plans (list, detail, create, cancel, pay installment).</summary>
[ApiController]
[Authorize]
[Route("api/v1/finance/installment-plans")]
public sealed class InstallmentPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public InstallmentPlansController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists installment plans for a finance module.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetInstallmentPlansResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetInstallmentPlansResponse>>> List(
        [FromQuery(Name = "status")] InstallmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new GetInstallmentPlansQuery(status), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<GetInstallmentPlansResponse> { Data = result });
    }

    /// <summary>Returns plan detail with pay schedule.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetInstallmentPlanDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetInstallmentPlanDetailResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetInstallmentPlanDetailQuery(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetInstallmentPlanDetailResponse> { Data = result });
    }

    /// <summary>Creates an installment plan from a deferred transaction.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateInstallmentPlanResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CreateInstallmentPlanResponse>>> Create(
        [FromBody] CreateInstallmentPlanCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<CreateInstallmentPlanResponse> { Data = result });
    }

    /// <summary>Cancels an active installment plan.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(
        Guid id,
        [FromBody] CancelInstallmentPlanBody? body,
        CancellationToken cancellationToken)
    {
        await _mediator
            .Send(new CancelInstallmentPlanCommand(id, body?.Reason), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { } });
    }

    /// <summary>Records payment for one installment period.</summary>
    [HttpPost("{id:guid}/pays/{number:int}/payment")]
    [ProducesResponseType(typeof(ApiResponse<RecordInstallmentPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RecordInstallmentPaymentResponse>>> PayInstallment(
        Guid id,
        int number,
        [FromBody] RecordInstallmentPaymentBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new RecordInstallmentPaymentCommand(id, number, body.PaymentSourceId), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<RecordInstallmentPaymentResponse> { Data = result });
    }
}

/// <summary>JSON body for <c>POST .../cancel</c>.</summary>
public sealed class CancelInstallmentPlanBody
{
    /// <summary>Optional cancellation reason.</summary>
    public string? Reason { get; set; }
}

/// <summary>JSON body for installment payment.</summary>
public sealed class RecordInstallmentPaymentBody
{
    /// <summary>Source to debit (bank / cash / e-wallet).</summary>
    public Guid PaymentSourceId { get; set; }
}
