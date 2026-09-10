using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFP.API.Models;
using PFP.Application.Features.Notifications.GetNotifications;
using PFP.Application.Features.Notifications.GetUnreadCount;
using PFP.Application.Features.Notifications.MarkAllNotificationsRead;
using PFP.Application.Features.Notifications.MarkNotificationRead;

namespace PFP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<GetNotificationsResponse>>> List(
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetNotificationsQuery(limit), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetNotificationsResponse> { Data = result });
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<GetUnreadCountResponse>>> UnreadCount(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<GetUnreadCountResponse> { Data = result });
    }

    [HttpPut("{notificationId:guid}/read")]
    public async Task<ActionResult<ApiResponse<MarkNotificationReadResponse>>> MarkRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MarkNotificationReadCommand(notificationId), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<MarkNotificationReadResponse> { Data = result });
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<MarkAllNotificationsReadResponse>>> MarkAllRead(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(new ApiResponse<MarkAllNotificationsReadResponse> { Data = result });
    }
}
