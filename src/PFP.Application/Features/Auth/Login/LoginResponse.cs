namespace PFP.Application.Features.Auth.Login;

/// <summary>Successful login payload including JWT access token and opaque refresh token.</summary>
public sealed record LoginResponse(
    Guid UserId,
    Guid OrganizationId,
    Guid SessionId,
    string Email,
    string FullName,
    bool IsEmailVerified,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
