using MediatR;

namespace PFP.Application.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest<MarkNotificationReadResponse>;
public sealed record MarkNotificationReadResponse(Guid NotificationId, bool IsRead);
