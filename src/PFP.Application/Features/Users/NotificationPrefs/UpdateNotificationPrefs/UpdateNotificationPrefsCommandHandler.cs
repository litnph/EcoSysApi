using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Users.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Users.NotificationPrefs.UpdateNotificationPrefs;

/// <summary>Upserts each preference cell (module × channel × event) supplied in the payload.</summary>
public sealed class UpdateNotificationPrefsCommandHandler : IRequestHandler<UpdateNotificationPrefsCommand, UpdateNotificationPrefsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateNotificationPrefsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<UpdateNotificationPrefsResponse> Handle(UpdateNotificationPrefsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var existing = await _db.UserNotificationPrefs
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var input in request.Preferences)
        {
            var eventType = input.EventType.Trim();
            var match = existing.FirstOrDefault(p =>
                p.ModuleCode == input.ModuleCode
                && p.Channel == input.Channel
                && p.EventType == eventType);

            if (match is null)
            {
                _db.UserNotificationPrefs.Add(new UserNotificationPref
                {
                    UserId = userId,
                    ModuleCode = input.ModuleCode,
                    Channel = input.Channel,
                    EventType = eventType,
                    IsEnabled = input.IsEnabled,
                });
            }
            else
            {
                match.IsEnabled = input.IsEnabled;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var refreshed = await _db.UserNotificationPrefs.AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.ModuleCode)
            .ThenBy(p => p.EventType)
            .ThenBy(p => p.Channel)
            .Select(p => new NotificationPrefDto(p.Id, p.ModuleCode, p.Channel, p.EventType, p.IsEnabled))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new UpdateNotificationPrefsResponse(refreshed);
    }
}
