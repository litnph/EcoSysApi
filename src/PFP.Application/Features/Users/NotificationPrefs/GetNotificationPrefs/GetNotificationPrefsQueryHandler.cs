using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.NotificationPrefs.GetNotificationPrefs;

/// <summary>Loads the caller's notification matrix (module × channel × event type).</summary>
public sealed class GetNotificationPrefsQueryHandler : IRequestHandler<GetNotificationPrefsQuery, GetNotificationPrefsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetNotificationPrefsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetNotificationPrefsResponse> Handle(GetNotificationPrefsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var rows = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .SelectMany(u => u.NotificationPreferences)
            .OrderBy(p => p.ModuleCode)
            .ThenBy(p => p.EventType)
            .ThenBy(p => p.Channel)
            .Select(p => new NotificationPrefDto(p.Id, p.ModuleCode, p.Channel, p.EventType, p.IsEnabled))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetNotificationPrefsResponse(rows);
    }
}
