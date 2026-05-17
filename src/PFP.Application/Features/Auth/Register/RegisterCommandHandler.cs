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

        await using (var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false))
        {

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

            var orgName = $"{fullName}'s Personal";
            var slug = await OrganizationSlugFactory.ReserveUniqueSlugAsync(_db, fullName, user.Id, cancellationToken)
                .ConfigureAwait(false);

            var organization = new Organization
            {
                IsPersonal = true,
                Slug = slug,
                Name = orgName,
                OwnerId = user.Id,
                DefaultCurrency = "VND",
            };
            _db.Organizations.Add(organization);

            var orgMember = new OrgMember
            {
                OrgId = organization.Id,
                UserId = user.Id,
                Role = OrgRole.Owner,
                IsActive = true,
                JoinedAt = now,
            };
            _db.OrgMembers.Add(orgMember);

            var rootSpace = new Space
            {
                OrgId = organization.Id,
                ParentId = null,
                Name = "Personal",
                Type = SpaceType.Personal,
                Path = $"/{organization.Id}",
                Depth = 0,
                SortOrder = 0,
            };
            _db.Spaces.Add(rootSpace);

            var spaceMember = new SpaceMember
            {
                SpaceId = rootSpace.Id,
                UserId = user.Id,
                Role = SpaceRole.Manager,
                Inherited = false,
                JoinedAt = now,
            };
            _db.SpaceMembers.Add(spaceMember);

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

            var response = new RegisterResponse(
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

            _emailDispatcher.DispatchEmailVerification(response.Email, response.FullName, plainVerify);

            return response;
        }
    }
}
