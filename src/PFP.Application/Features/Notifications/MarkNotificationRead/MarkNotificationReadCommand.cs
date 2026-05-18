using MediatR;

namespace PFP.Application.Features.Notifications.MarkNotificationRead;

/// <summary>Marks a single notification as read.</summary>
public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<MarkNotificationReadResponse>;

/// <summary>Response containing the updated unread count.</summary>
public sealed record MarkNotificationReadResponse(Guid NotificationId, int UnreadCount);
