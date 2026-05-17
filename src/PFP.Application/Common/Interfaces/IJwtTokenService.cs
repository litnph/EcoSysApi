using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Models;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Issues and validates JWT access tokens and orchestrates refresh-token rotation against
/// <c>USER_SESSIONS</c> (spec §6.1).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Builds a signed JWT containing <paramref name="userId"/>, <paramref name="sessionId"/>, and <paramref name="orgId"/> (15-minute lifetime).</summary>
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Guid userId, Guid sessionId, Guid orgId);

    /// <summary>
    /// Generates a cryptographically random refresh token, its SHA-256 digest for persistence, and the UTC expiry (30 days).
    /// </summary>
    RefreshTokenCredentials CreateRefreshTokenCredentials();

    /// <summary>
    /// Validates an access JWT using the same rules as the API bearer middleware (signature, iss, aud, lifetime).
    /// </summary>
    /// <param name="token">Raw JWT string (without <c>Bearer</c> prefix).</param>
    JwtTokenValidationResult ValidateAccessToken(string token);

    /// <summary>
    /// Rotates refresh material for the matching active session and returns a new access token.
    /// Uses the EF execution strategy so transient database failures are retried safely.
    /// </summary>
    /// <param name="plainRefreshToken">Opaque refresh token presented by the client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="UnauthorizedAppException">Thrown when the refresh token is unknown, revoked, or expired.</exception>
    Task<RefreshTokenExchangeResult> ExchangeRefreshTokenAsync(string plainRefreshToken, CancellationToken cancellationToken = default);
}
