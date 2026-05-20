using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Constants;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Application.Features.Auth.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Auth.Register;

/// <summary>Executes the full registration bootstrap inside a single database transaction (spec §4.1).</summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuthEmailDispatcher _emailDispatcher;
    private readonly IClientRequestContext _client;

    /// <summary>Creates the handler.</summary>
    public RegisterCommandHandler(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ITokenHasher tokenHasher,
        IAuthEmailDispatcher emailDispatcher,
        IClientRequestContext client)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _tokenHasher = tokenHasher;
        _emailDispatcher = emailDispatcher;
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = AuthEmailNormalizer.Normalize(request.Email);
        var fullName = request.FullName.Trim();

        var passwordHash = _passwordHasher.Hash(request.Password);
        var plainVerify = SecureTokenGenerator.CreateUrlSafe();
        var verifyHash = _tokenHasher.Sha256Hex(plainVerify);

        var refreshCreds = _jwtTokenService.CreateRefreshTokenCredentials();
        var plainRefresh = refreshCreds.PlainRefreshToken;
        var refreshHash = refreshCreds.RefreshTokenSha256Hex;
        var refreshExpires = refreshCreds.ExpiresAtUtc;

        var now = DateTime.UtcNow;
        var verifyExpires = now.AddHours(AuthConstants.EmailVerificationTokenLifetimeHours);

        var strategy = _db.Database.CreateExecutionStrategy();
        var response = await strategy
            .ExecuteAsync(
                async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                    var user = new User
                    {
                        Email = email,
                        PasswordHash = passwordHash,
                        FullName = fullName,
                        IsEmailVerified = false,
                    };

                    _db.Users.Add(user);

                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        LanguageCode = "vi",
                        Timezone = "Asia/Ho_Chi_Minh",
                    };
                    _db.UserProfiles.Add(profile);

                    var emailProvider = new UserAuthProvider
                    {
                        UserId = user.Id,
                        Provider = AuthProvider.Email,
                        ProviderUserId = email,
                        ProviderEmail = email,
                        IsActive = true,
                        LinkedAt = now,
                    };
                    _db.UserAuthProviders.Add(emailProvider);

                    var bootstrap = await PersonalOrganizationBootstrap
                        .ProvisionAsync(_db, user.Id, fullName, now, cancellationToken)
                        .ConfigureAwait(false);
                    var organization = bootstrap.Organization;
                    var rootSpace = bootstrap.RootSpace;

                    var emailVerification = new UserEmailVerification
                    {
                        UserId = user.Id,
                        Type = EmailVerificationType.VerifyEmail,
                        TokenHash = verifyHash,
                        ExpiresAt = verifyExpires,
                    };
                    _db.UserEmailVerifications.Add(emailVerification);

                    var session = new UserSession
                    {
                        UserId = user.Id,
                        TokenHash = refreshHash,
                        ExpiresAt = refreshExpires,
                        LastUsedAt = now,
                        IpAddress = _client.IpAddress,
                        UserAgent = _client.UserAgent,
                    };
                    _db.UserSessions.Add(session);

                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var (accessToken, accessExpires) = _jwtTokenService.CreateAccessToken(user.Id, session.Id, organization.Id);

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return new RegisterResponse(
                        user.Id,
                        organization.Id,
                        rootSpace.Id,
                        session.Id,
                        user.Email,
                        user.FullName,
                        user.IsEmailVerified,
                        accessToken,
                        plainRefresh,
                        accessExpires,
                        refreshExpires);
                })
            .ConfigureAwait(false);

        _emailDispatcher.DispatchEmailVerification(response.Email, response.FullName, plainVerify);

        return response;
    }
}
