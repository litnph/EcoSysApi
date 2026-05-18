using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Notifications.DeleteNotification;

/// <summary>Hard-deletes one notification row owned by the caller.</summary>
public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, DeleteNotificationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public DeleteNotificationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<DeleteNotificationResponse> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var n = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == request.NotificationId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (n is null)
            throw new NotFoundException("Notification was not found.");

        _db.Notifications.Remove(n);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteNotificationResponse(request.NotificationId);
    }
}
