using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Models;

namespace PFP.Infrastructure.Identity;

/// <summary>
/// HS256 access tokens, refresh-token material generation, JWT validation, and refresh exchange with
/// EF execution-strategy retries.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IApplicationDbContext _db;
    private readonly ITokenHasher _tokenHasher;
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <summary>Creates the service.</summary>
    public JwtTokenService(IOptions<JwtOptions> options, IApplicationDbContext db, ITokenHasher tokenHasher)
    {
        _options = options.Value;
        _db = db;
        _tokenHasher = tokenHasher;
    }

    /// <inheritdoc/>
    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(Guid userId, Guid sessionId, Guid orgId)
    {
        EnsureSecret();

        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(AuthConstants.AccessTokenLifetimeMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.SessionId, sessionId.ToString()),
            new Claim(JwtClaimNames.OrgId, orgId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var creds = new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        return (_handler.WriteToken(token), expires);
    }

    /// <inheritdoc/>
    public RefreshTokenCredentials CreateRefreshTokenCredentials()
    {
        var plain = CryptoToken.CreateUrlSafe();
        var hash = _tokenHasher.Sha256Hex(plain);
        var expires = DateTime.UtcNow.AddDays(AuthConstants.RefreshTokenLifetimeDays);
        return new RefreshTokenCredentials(plain, hash, expires);
    }

    /// <inheritdoc/>
    public JwtTokenValidationResult ValidateAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return JwtTokenValidationResult.Failed("Token is missing.");

        try
        {
            EnsureSecret();
            var principal = _handler.ValidateToken(token, BuildValidationParameters(), out _);
            return JwtTokenValidationResult.Success(principal);
        }
        catch (SecurityTokenException ex)
        {
            return JwtTokenValidationResult.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return JwtTokenValidationResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RefreshTokenExchangeResult> ExchangeRefreshTokenAsync(
        string plainRefreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plainRefreshToken))
            throw new UnauthorizedAppException("Refresh token is invalid or expired.");

        var hash = _tokenHasher.Sha256Hex(plainRefreshToken);

        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                var session = await _db.UserSessions
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(
                        s => s.TokenHash == hash && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow,
                        cancellationToken)
                    .ConfigureAwait(false);

                if (session is null)
                    throw new UnauthorizedAppException("Refresh token is invalid or expired.");

                if (session.User is null || !session.User.IsActive)
                    throw new UnauthorizedAppException("Refresh token is invalid or expired.");

                var personalOrgId = await _db.Organizations
                    .AsNoTracking()
                    .Where(o => o.OwnerId == session.UserId && o.IsPersonal)
                    .Select(o => o.Id)
                    .FirstAsync(cancellationToken)
                    .ConfigureAwait(false);

                var creds = CreateRefreshTokenCredentials();
                var now = DateTime.UtcNow;
                session.TokenHash = creds.RefreshTokenSha256Hex;
                session.ExpiresAt = creds.ExpiresAtUtc;
                session.LastUsedAt = now;

                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                var (accessToken, accessExpires) = CreateAccessToken(session.UserId, session.Id, personalOrgId);

                return new RefreshTokenExchangeResult(
                    session.UserId,
                    personalOrgId,
                    session.Id,
                    accessToken,
                    creds.PlainRefreshToken,
                    accessExpires,
                    session.ExpiresAt);
            }).ConfigureAwait(false);
    }

    private void EnsureSecret()
    {
        if (string.IsNullOrWhiteSpace(_options.Secret) || _options.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be configured with at least 32 characters.");
    }

    private SymmetricSecurityKey GetSigningKey() =>
        new(Encoding.UTF8.GetBytes(_options.Secret));

    private TokenValidationParameters BuildValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = GetSigningKey(),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
}
