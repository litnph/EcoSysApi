namespace PFP.Application.Common.Models;

/// <summary>Payload returned after a successful refresh-token exchange.</summary>
public sealed record RefreshTokenExchangeResult(
    Guid UserId,
    Guid SessionId,
    string AccessToken,
    string PlainRefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
