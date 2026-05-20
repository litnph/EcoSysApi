namespace PFP.Application.Features.Auth.RefreshToken;

/// <summary>Rotated credentials returned from <c>POST /auth/refresh</c>.</summary>
public sealed record RefreshTokenResponse(
    Guid UserId,
    Guid SessionId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
