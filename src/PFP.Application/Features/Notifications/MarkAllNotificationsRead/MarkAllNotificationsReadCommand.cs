using MediatR;

namespace PFP.Application.Features.Notifications.MarkAllNotificationsRead;

/// <summary>Marks every unread notification of the caller as read.</summary>
public sealed record MarkAllNotificationsReadCommand() : IRequest<MarkAllNotificationsReadResponse>;

/// <summary>Response containing how many rows changed.</summary>
public sealed record MarkAllNotificationsReadResponse(int Updated);
