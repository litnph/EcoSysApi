using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Notifications.Common;

namespace PFP.Application.Features.Notifications.GetNotifications;

/// <summary>Returns the caller's notifications with pagination and an unread-count tally.</summary>
public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var baseQuery = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);

        if (request.IsRead is { } isRead)
            baseQuery = baseQuery.Where(n => n.IsRead == isRead);

        var total = await baseQuery.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await baseQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new NotificationItemDto(n.Id, n.Type, n.Title, n.Body, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var unread = await _db.Notifications.AsNoTracking()
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken)
            .ConfigureAwait(false);

        var pages = request.PageSize > 0
            ? (int)Math.Ceiling(total / (double)request.PageSize)
            : 0;

        return new GetNotificationsResponse(items, request.Page, request.PageSize, total, pages, unread);
    }
}
