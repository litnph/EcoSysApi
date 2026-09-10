using MediatR;
using PFP.Application.Features.Notifications.Common;

namespace PFP.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(int Limit = 50) : IRequest<GetNotificationsResponse>;
public sealed record GetNotificationsResponse(IReadOnlyList<NotificationDto> Items, int UnreadCount);
