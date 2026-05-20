using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Models;

namespace PFP.Application.Common.Interfaces;

/// <summary>Issues and validates JWT access tokens and refresh-token rotation.</summary>
public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Guid userId, Guid sessionId);

    RefreshTokenCredentials CreateRefreshTokenCredentials();

    JwtTokenValidationResult ValidateAccessToken(string token);

    Task<RefreshTokenExchangeResult> ExchangeRefreshTokenAsync(
        string plainRefreshToken,
        CancellationToken cancellationToken = default);
}
