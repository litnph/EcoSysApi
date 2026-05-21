using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Auth.Login;

/// <summary>Handles password login and session issuance.</summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClientRequestContext _client;

    public LoginCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IClientRequestContext client)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _client = client;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = AuthEmailNormalizer.Normalize(request.Email);
        var now = DateTime.UtcNow;

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAppException("Invalid email or password.");

        if (string.IsNullOrEmpty(user.PasswordHash)
            || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedAppException("Invalid email or password.");

        var refreshCreds = _jwtTokenService.CreateRefreshTokenCredentials();
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var tracked = await _db.Users.FirstAsync(u => u.Id == user.Id, cancellationToken).ConfigureAwait(false);
            tracked.LastLoginAt = now;

            var session = new UserSession
            {
                UserId = tracked.Id,
                TokenHash = refreshCreds.RefreshTokenSha256Hex,
                ExpiresAt = refreshCreds.ExpiresAtUtc,
                LastUsedAt = now,
                IpAddress = _client.IpAddress,
                UserAgent = _client.UserAgent,
            };
            _db.UserSessions.Add(session);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            var (accessToken, accessExpires) = _jwtTokenService.CreateAccessToken(tracked.Id, session.Id);

            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new LoginResponse(
                tracked.Id,
                session.Id,
                tracked.Email,
                tracked.FullName,
                tracked.Role,
                accessToken,
                refreshCreds.PlainRefreshToken,
                accessExpires,
                refreshCreds.ExpiresAtUtc);
        }).ConfigureAwait(false);
    }
}
