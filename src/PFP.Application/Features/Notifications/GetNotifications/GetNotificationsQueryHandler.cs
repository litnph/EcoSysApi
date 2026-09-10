using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Notifications.Common;

namespace PFP.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBudgetAlertEvaluator _alerts;

    public GetNotificationsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IBudgetAlertEvaluator alerts)
    {
        _db = db;
        _currentUser = currentUser;
        _alerts = alerts;
    }

    public async Task<GetNotificationsResponse> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
        var userId = _currentUser.UserId.Value;

        await _alerts.EvaluateCurrentCycleAsync(userId, cancellationToken).ConfigureAwait(false);

        var items = await _db.UserNotifications
            .AsNoTracking()
            .Where(row => row.UserId == userId)
            .OrderByDescending(row => row.CreatedAt)
            .Take(request.Limit)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var unread = await _db.UserNotifications
            .AsNoTracking()
            .CountAsync(row => row.UserId == userId && !row.IsRead, cancellationToken)
            .ConfigureAwait(false);
        return new GetNotificationsResponse(items.Select(NotificationDto.FromEntity).ToList(), unread);
    }
}
