using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Auth;
using PFP.Application.Features.Auth.Common;
using PFP.Application.Features.Auth.Login;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Auth.Google;

/// <summary>Links or provisions a user from Google OAuth and issues JWT + refresh session.</summary>
public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, LoginResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IClientRequestContext _client;

    /// <summary>Creates the handler.</summary>
    public GoogleLoginCommandHandler(
        IApplicationDbContext db,
        IJwtTokenService jwtTokenService,
        IClientRequestContext client)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _client = client;
    }

    /// <inheritdoc/>
    public async Task<LoginResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var email = AuthEmailNormalizer.Normalize(request.Email);
        var googleSub = request.GoogleSubject.Trim();
        var fullName = request.FullName.Trim();
        var now = DateTime.UtcNow;

        var googleProvider = await _db.UserAuthProviders
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.Provider == AuthProvider.Google && p.ProviderUserId == googleSub,
                cancellationToken)
            .ConfigureAwait(false);

        if (googleProvider is not null)
        {
            googleProvider.IsActive = true;
            googleProvider.LastUsedAt = now;
            googleProvider.ProviderEmail = email;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return await IssueSessionForUserAsync(googleProvider.User, cancellationToken).ConfigureAwait(false);
        }

        var existingUser = await _db.Users
            .Include(u => u.AuthProviders)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            .ConfigureAwait(false);

        if (existingUser is not null)
        {
            var otherGoogle = existingUser.AuthProviders.FirstOrDefault(
                p => p.Provider == AuthProvider.Google && p.IsActive && p.ProviderUserId != googleSub);

            if (otherGoogle is not null)
            {
                throw new BusinessRuleException(
                    "This email address is already linked to a different Google account.");
            }

            var sameGoogleInactive = existingUser.AuthProviders.FirstOrDefault(
                p => p.Provider == AuthProvider.Google && p.ProviderUserId == googleSub);

            if (sameGoogleInactive is not null)
            {
                sameGoogleInactive.IsActive = true;
                sameGoogleInactive.LastUsedAt = now;
                sameGoogleInactive.ProviderEmail = email;
            }
            else
            {
                _db.UserAuthProviders.Add(
                    new UserAuthProvider
                    {
                        UserId = existingUser.Id,
                        Provider = AuthProvider.Google,
                        ProviderUserId = googleSub,
                        ProviderEmail = email,
                        IsActive = true,
                        LinkedAt = now,
                        LastUsedAt = now,
                    });
            }

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return await IssueSessionForUserAsync(existingUser, cancellationToken).ConfigureAwait(false);
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy
            .ExecuteAsync(
                async () =>
                {
                    await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

                    var user = new User
                    {
                        Email = email,
                        PasswordHash = null,
                        FullName = fullName,
                        IsEmailVerified = true,
                    };
                    _db.Users.Add(user);

                    _db.UserProfiles.Add(
                        new UserProfile
                        {
                            UserId = user.Id,
                            LanguageCode = "vi",
                            Timezone = "Asia/Ho_Chi_Minh",
                        });

                    _db.UserAuthProviders.Add(
                        new UserAuthProvider
                        {
                            UserId = user.Id,
                            Provider = AuthProvider.Google,
                            ProviderUserId = googleSub,
                            ProviderEmail = email,
                            IsActive = true,
                            LinkedAt = now,
                            LastUsedAt = now,
                        });

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

                    _db.OrgMembers.Add(
                        new OrgMember
                        {
                            OrgId = organization.Id,
                            UserId = user.Id,
                            Role = OrgRole.Owner,
                            IsActive = true,
                            JoinedAt = now,
                        });

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

                    _db.SpaceMembers.Add(
                        new SpaceMember
                        {
                            SpaceId = rootSpace.Id,
                            UserId = user.Id,
                            Role = SpaceRole.Manager,
                            Inherited = false,
                            JoinedAt = now,
                        });

                    var refresh = _jwtTokenService.CreateRefreshTokenCredentials();
                    var session = new UserSession
                    {
                        UserId = user.Id,
                        TokenHash = refresh.RefreshTokenSha256Hex,
                        ExpiresAt = refresh.ExpiresAtUtc,
                        LastUsedAt = now,
                        IpAddress = _client.IpAddress,
                        UserAgent = _client.UserAgent,
                    };
                    _db.UserSessions.Add(session);

                    user.LastLoginAt = now;

                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var (accessToken, accessExpires) =
                        _jwtTokenService.CreateAccessToken(user.Id, session.Id, organization.Id);

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return new LoginResponse(
                        user.Id,
                        organization.Id,
                        session.Id,
                        user.Email,
                        user.FullName,
                        user.IsEmailVerified,
                        accessToken,
                        refresh.PlainRefreshToken,
                        accessExpires,
                        refresh.ExpiresAtUtc);
                })
            .ConfigureAwait(false);
    }

    private async Task<LoginResponse> IssueSessionForUserAsync(User user, CancellationToken cancellationToken)
    {
        if (!user.IsActive)
            throw new UnauthorizedAppException("This account is disabled.");

        var now = DateTime.UtcNow;
        var personalOrgId = await _db.Organizations
            .AsNoTracking()
            .Where(o => o.OwnerId == user.Id && o.IsPersonal)
            .Select(o => o.Id)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        var refresh = _jwtTokenService.CreateRefreshTokenCredentials();

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
                        TokenHash = refresh.RefreshTokenSha256Hex,
                        ExpiresAt = refresh.ExpiresAtUtc,
                        LastUsedAt = now,
                        IpAddress = _client.IpAddress,
                        UserAgent = _client.UserAgent,
                    };
                    _db.UserSessions.Add(session);

                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    var (accessToken, accessExpires) =
                        _jwtTokenService.CreateAccessToken(tracked.Id, session.Id, personalOrgId);

                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                    return new LoginResponse(
                        tracked.Id,
                        personalOrgId,
                        session.Id,
                        tracked.Email,
                        tracked.FullName,
                        tracked.IsEmailVerified,
                        accessToken,
                        refresh.PlainRefreshToken,
                        accessExpires,
                        refresh.ExpiresAtUtc);
                })
            .ConfigureAwait(false);
    }
}
