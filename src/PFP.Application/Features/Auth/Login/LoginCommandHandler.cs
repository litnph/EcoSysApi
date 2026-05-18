using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Auth.Login;

/// <summary>Handles password login, brute-force bookkeeping, and session issuance.</summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClientRequestContext _client;

    /// <summary>Creates the handler.</summary>
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

    /// <inheritdoc/>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = AuthEmailNormalizer.Normalize(request.Email);
        var now = DateTime.UtcNow;

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            await AppendLoginAttemptAsync(
                    null,
                    email,
                    isSuccess: false,
                    LoginFailureReasons.UserNotFound,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            await AppendLoginAttemptAsync(
                    user.Id,
                    email,
                    isSuccess: false,
                    LoginFailureReasons.UserNotFound,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (await IsAccountLockedAsync(user.Id, cancellationToken).ConfigureAwait(false))
        {
            await AppendLoginAttemptAsync(
                    user.Id,
                    email,
                    isSuccess: false,
                    LoginFailureReasons.AccountLocked,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new UnauthorizedAppException("This account is temporarily locked. Try again later.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            await AppendLoginAttemptAsync(
                    user.Id,
                    email,
                    isSuccess: false,
                    LoginFailureReasons.MissingPassword,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            await AppendLoginAttemptAsync(
                    user.Id,
                    email,
                    isSuccess: false,
                    LoginFailureReasons.InvalidPassword,
                    cancellationToken)
                .ConfigureAwait(false);
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        var personalOrgId = await _db.Organizations
            .AsNoTracking()
            .Where(o => o.OwnerId == user.Id && o.IsPersonal)
            .Select(o => o.Id)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        var refreshCreds = _jwtTokenService.CreateRefreshTokenCredentials();
        var plainRefresh = refreshCreds.PlainRefreshToken;
        var refreshHash = refreshCreds.RefreshTokenSha256Hex;
        var refreshExpires = refreshCreds.ExpiresAtUtc;

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy
            .ExecuteAsync(
                async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                    var tracked = await _db.Users.FirstAsync(u => u.Id == user.Id, cancellationToken).ConfigureAwait(false);
                    tracked.LastLoginAt = now;

                    var session = new UserSession
                    {
                        UserId = tracked.Id,
                        TokenHash = refreshHash,
                        ExpiresAt = refreshExpires,
                        LastUsedAt = now,
                        IpAddress = _client.IpAddress,
                        UserAgent = _client.UserAgent,
                    };
                    _db.UserSessions.Add(session);

                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var (accessToken, accessExpires) =
                        _jwtTokenService.CreateAccessToken(tracked.Id, session.Id, personalOrgId);

                    await AppendLoginAttemptAsync(tracked.Id, email, isSuccess: true, null, cancellationToken)
                        .ConfigureAwait(false);

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return new LoginResponse(
                        tracked.Id,
                        personalOrgId,
                        session.Id,
                        tracked.Email,
                        tracked.FullName,
                        tracked.IsEmailVerified,
                        accessToken,
                        plainRefresh,
                        accessExpires,
                        refreshExpires);
                })
            .ConfigureAwait(false);
    }

    private async Task<bool> IsAccountLockedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var recent = await _db.UserLoginAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(32)
            .Select(a => new { a.IsSuccess, a.CreatedAt })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var streak = 0;
        foreach (var row in recent)
        {
            if (row.IsSuccess)
                break;
            streak++;
        }

        if (streak < AuthConstants.MaxFailedLoginAttemptsInWindow)
            return false;

        var lastFailureAt = recent[0].CreatedAt;
        return lastFailureAt.Add(AuthConstants.AccountLockDuration) > DateTime.UtcNow;
    }

    private async Task AppendLoginAttemptAsync(
        Guid? userId,
        string attemptedEmail,
        bool isSuccess,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        _db.UserLoginAttempts.Add(
            new UserLoginAttempt
            {
                UserId = userId,
                AttemptedEmail = attemptedEmail,
                IsSuccess = isSuccess,
                FailureReason = failureReason,
                IpAddress = _client.IpAddress,
                UserAgent = _client.UserAgent,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
