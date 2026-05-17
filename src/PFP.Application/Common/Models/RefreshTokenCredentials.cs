namespace PFP.Application.Common.Models;

/// <summary>Opaque refresh token material for <c>USER_SESSIONS</c> (plain token is returned once to the client).</summary>
public sealed record RefreshTokenCredentials(
    string PlainRefreshToken,
    string RefreshTokenSha256Hex,
    DateTime ExpiresAtUtc);
