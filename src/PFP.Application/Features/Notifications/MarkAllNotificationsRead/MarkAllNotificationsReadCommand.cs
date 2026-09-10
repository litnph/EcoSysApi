using MediatR;

namespace PFP.Application.Features.Notifications.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand : IRequest<MarkAllNotificationsReadResponse>;
public sealed record MarkAllNotificationsReadResponse(int UpdatedCount);
