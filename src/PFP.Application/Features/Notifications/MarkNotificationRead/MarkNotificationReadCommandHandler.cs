using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler
    : IRequestHandler<MarkNotificationReadCommand, MarkNotificationReadResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    public MarkNotificationReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MarkNotificationReadResponse> Handle(
        MarkNotificationReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
        var row = await _db.UserNotifications.FirstOrDefaultAsync(
            item => item.Id == request.NotificationId && item.UserId == _currentUser.UserId.Value,
            cancellationToken).ConfigureAwait(false);
        if (row is null)
            throw new NotFoundException("Notification was not found.");
        if (!row.IsRead)
        {
            row.IsRead = true;
            row.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        return new MarkNotificationReadResponse(row.Id, true);
    }
}
