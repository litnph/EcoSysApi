using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Notifications.Common;

namespace PFP.Application.Features.Notifications.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, GetUnreadCountResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IBudgetAlertEvaluator _alerts;

    public GetUnreadCountQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IBudgetAlertEvaluator alerts)
    {
        _db = db;
        _currentUser = currentUser;
        _alerts = alerts;
    }

    public async Task<GetUnreadCountResponse> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
        var userId = _currentUser.UserId.Value;
        await _alerts.EvaluateCurrentCycleAsync(userId, cancellationToken).ConfigureAwait(false);
        var count = await _db.UserNotifications.AsNoTracking()
            .CountAsync(row => row.UserId == userId && !row.IsRead, cancellationToken)
            .ConfigureAwait(false);
        return new GetUnreadCountResponse(count);
    }
}
