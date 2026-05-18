using MediatR;
using PFP.Application.Features.Users.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Users.NotificationPrefs.UpdateNotificationPrefs;

/// <summary>
/// Replaces every preference toggle the caller is sending with the supplied value (upserts by
/// <c>(module_code, channel, event_type)</c>). Items not included in the payload are left alone.
/// </summary>
public sealed record UpdateNotificationPrefsCommand(IReadOnlyList<NotificationPrefInput> Preferences)
    : IRequest<UpdateNotificationPrefsResponse>;

/// <summary>Single matrix cell sent by the client.</summary>
public sealed record NotificationPrefInput(
    ModuleCode ModuleCode,
    NotificationChannel Channel,
    string EventType,
    bool IsEnabled);

/// <summary>Response wrapper containing the refreshed matrix.</summary>
public sealed record UpdateNotificationPrefsResponse(IReadOnlyList<NotificationPrefDto> Preferences);
