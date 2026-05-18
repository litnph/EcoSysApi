using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Notifications.DeleteNotification;
using PFP.Application.Features.Notifications.GetNotifications;
using PFP.Application.Features.Notifications.MarkAllNotificationsRead;
using PFP.Application.Features.Notifications.MarkNotificationRead;

namespace PFP.API.Controllers;

/// <summary>In-app notification centre endpoints (list / mark as read / mark all as read / delete).</summary>
[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Creates the controller.</summary>
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns the caller's notifications, newest first, with an unread tally.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<GetNotificationsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GetNotificationsResponse>>> List(
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page_size")] int pageSize = 20,
        [FromQuery(Name = "is_read")] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(page, pageSize, isRead), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetNotificationsResponse> { Data = result });
    }

    /// <summary>Marks one notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<MarkNotificationReadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MarkNotificationReadResponse>>> MarkRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<MarkNotificationReadResponse> { Data = result });
    }

    /// <summary>Marks every unread notification as read.</summary>
    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ApiResponse<MarkAllNotificationsReadResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MarkAllNotificationsReadResponse>>> MarkAllRead(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<MarkAllNotificationsReadResponse> { Data = result });
    }

    /// <summary>Deletes one notification.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteNotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeleteNotificationResponse>>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteNotificationCommand(id), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<DeleteNotificationResponse> { Data = result });
    }
}
