namespace PFP.Application.Common.Models;

/// <summary>Payload returned after a successful refresh-token exchange (includes new opaque refresh token).</summary>
public sealed record RefreshTokenExchangeResult(
    Guid UserId,
    Guid OrganizationId,
    Guid SessionId,
    string AccessToken,
    string PlainRefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
