using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Gdpr.DeleteAccount;

namespace PFP.API.Controllers;

/// <summary>GDPR account deletion with grace period.</summary>
[ApiController]
[Route("api/v1/user")]
public sealed class AccountDeletionController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public AccountDeletionController(IMediator mediator) => _mediator = mediator;

    /// <summary>Creates a pending deletion request and emails a confirmation link.</summary>
    [Authorize]
    [HttpPost("deletion-request")]
    [ProducesResponseType(typeof(ApiResponse<RequestAccountDeletionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RequestAccountDeletionResponse>>> RequestDeletion(
        [FromBody] RequestAccountDeletionBody body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator
            .Send(new RequestAccountDeletionCommand(body.Reason), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new ApiResponse<RequestAccountDeletionResponse> { Data = result });
    }

    /// <summary>Confirms deletion from the emailed token.</summary>
    [AllowAnonymous]
    [HttpPost("deletion-request/confirm")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Confirm(
        [FromBody] ConfirmAccountDeletionBody body,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmAccountDeletionCommand(body.Token), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { confirmed = true } });
    }

    /// <summary>Cancels before the scheduled execution time.</summary>
    [Authorize]
    [HttpPost("deletion-request/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelAccountDeletionCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<object> { Data = new { cancelled = true } });
    }
}
