using PFP.Domain.Enums;

namespace PFP.Application.Features.Users.Common;

/// <summary>Compact "who am I" payload for <c>GET /user/me</c>.</summary>
public sealed record UserMeDto(
    Guid UserId,
    string Email,
    string FullName,
    UserRole Role,
    DateTime? LastLoginAt,
    string LanguageCode,
    string Timezone,
    string Theme,
    string? AvatarUrl);

/// <summary>Profile + preferences row.</summary>
public sealed record UserProfileDto(
    Guid UserId,
    string FullName,
    string Email,
    UserRole Role,
    string LanguageCode,
    string Timezone,
    string DateFormat,
    string Theme,
    string? DisplayName,
    string? PhoneNumber,
    DateOnly? DateOfBirth,
    string? AvatarUrl);

/// <summary>Granular notification-preference row.</summary>
public sealed record NotificationPrefDto(
    Guid Id,
    ModuleCode ModuleCode,
    NotificationChannel Channel,
    string EventType,
    bool IsEnabled);
