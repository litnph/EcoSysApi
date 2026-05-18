namespace PFP.Application.Features.Notifications.Common;

/// <summary>Compact notification row for the bell-icon dropdown / centre.</summary>
public sealed record NotificationItemDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedAt);
