using MediatR;

namespace PFP.Application.Features.Notifications.DeleteNotification;

/// <summary>Permanently removes a notification from the caller's notification centre.</summary>
public sealed record DeleteNotificationCommand(Guid NotificationId) : IRequest<DeleteNotificationResponse>;

/// <summary>Response acknowledging the removal.</summary>
public sealed record DeleteNotificationResponse(Guid NotificationId);
