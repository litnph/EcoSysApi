using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Notifications.MarkAllNotificationsRead;

/// <summary>Bulk mark-as-read using <c>ExecuteUpdateAsync</c> to keep the operation O(1) round-trips.</summary>
public sealed class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public MarkAllNotificationsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<MarkAllNotificationsReadResponse> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        // Server-side UPDATE — one round-trip instead of N+1 (load every unread row, mutate, save).
        var updated = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), cancellationToken)
            .ConfigureAwait(false);

        return new MarkAllNotificationsReadResponse(updated);
    }
}
