using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Notifications.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MarkAllNotificationsReadResponse> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
        var rows = await _db.UserNotifications
            .Where(item => item.UserId == _currentUser.UserId.Value && !item.IsRead)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var now = DateTime.UtcNow;
        foreach (var row in rows)
        {
            row.IsRead = true;
            row.ReadAt = now;
        }
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return new MarkAllNotificationsReadResponse(rows.Count);
    }
}
