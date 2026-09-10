using MediatR;

namespace PFP.Application.Features.Notifications.GetUnreadCount;

public sealed record GetUnreadCountQuery : IRequest<GetUnreadCountResponse>;
public sealed record GetUnreadCountResponse(int UnreadCount);
