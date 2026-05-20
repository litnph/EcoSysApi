using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
namespace PFP.Application.Features.Auth.SwitchOrganization;

/// <summary>
/// Validates org membership, updates <see cref="Domain.Entities.UserSession.ActiveOrgId"/>,
/// and rotates refresh material with a new access token for the target org.
/// </summary>
public sealed class SwitchOrganizationCommandHandler
    : IRequestHandler<SwitchOrganizationCommand, SwitchOrganizationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>Creates the handler.</summary>
    public SwitchOrganizationCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IJwtTokenService jwtTokenService)
    {
        _db = db;
        _currentUser = currentUser;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc/>
    public async Task<SwitchOrganizationResponse> Handle(
        SwitchOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated
            || _currentUser.UserId is null
            || _currentUser.SessionId is null)
        {
            throw new UnauthorizedAppException("Authentication is required.");
        }

        var userId = _currentUser.UserId.Value;
        var sessionId = _currentUser.SessionId.Value;
        var orgId = request.OrganizationId;

        var isMember = await _db.OrgMembers
            .AsNoTracking()
            .AnyAsync(
                m => m.OrgId == orgId && m.UserId == userId && m.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this organisation.");

        var orgExists = await _db.Organizations
            .AsNoTracking()
            .AnyAsync(o => o.Id == orgId, cancellationToken)
            .ConfigureAwait(false);

        if (!orgExists)
            throw new NotFoundException("Organisation not found.");

        var result = await _jwtTokenService
            .SwitchOrganizationAsync(sessionId, userId, orgId, cancellationToken)
            .ConfigureAwait(false);

        return new SwitchOrganizationResponse(
            result.UserId,
            result.OrganizationId,
            result.SessionId,
            result.AccessToken,
            result.PlainRefreshToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshTokenExpiresAtUtc);
    }
}
