using MediatR;
using PFP.Application.Features.Users.Common;

namespace PFP.Application.Features.Users.NotificationPrefs.GetNotificationPrefs;

/// <summary>Returns every <c>USER_NOTIFICATION_PREFS</c> row for the caller.</summary>
public sealed record GetNotificationPrefsQuery() : IRequest<GetNotificationPrefsResponse>;

/// <summary>Response wrapper.</summary>
public sealed record GetNotificationPrefsResponse(IReadOnlyList<NotificationPrefDto> Preferences);
