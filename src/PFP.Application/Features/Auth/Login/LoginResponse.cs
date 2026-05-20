using PFP.Domain.Enums;

namespace PFP.Application.Features.Auth.Login;

/// <summary>Successful login payload including JWT access token and opaque refresh token.</summary>
public sealed record LoginResponse(
    Guid UserId,
    Guid SessionId,
    string Email,
    string FullName,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
