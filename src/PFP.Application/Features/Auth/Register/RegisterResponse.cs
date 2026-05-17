namespace PFP.Application.Features.Auth.Register;

/// <summary>Successful registration payload including first-session JWT pair.</summary>
public sealed record RegisterResponse(
    Guid UserId,
    Guid OrganizationId,
    Guid PersonalSpaceId,
    Guid SessionId,
    string Email,
    string FullName,
    bool IsEmailVerified,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
