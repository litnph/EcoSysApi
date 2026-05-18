using MediatR;
using PFP.Application.Features.Notifications.Common;

namespace PFP.Application.Features.Notifications.GetNotifications;

/// <summary>Paginated list of the caller's in-app notifications, newest first.</summary>
public sealed record GetNotificationsQuery(int Page, int PageSize, bool? IsRead) : IRequest<GetNotificationsResponse>;

/// <summary>Paginated envelope.</summary>
public sealed record GetNotificationsResponse(
    IReadOnlyList<NotificationItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    int UnreadCount);
