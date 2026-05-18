using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.LeaveOrganization;

/// <summary>
/// Marks the caller's <c>OrgMember</c> row inactive and ends every space membership inside the
/// same organisation. Owners must transfer ownership first.
/// </summary>
public sealed class LeaveOrganizationCommandHandler : IRequestHandler<LeaveOrganizationCommand, LeaveOrganizationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public LeaveOrganizationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<LeaveOrganizationResponse> Handle(LeaveOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var org = await _db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (org is null)
            throw new NotFoundException("Organisation was not found.");

        if (org.IsPersonal)
            throw new BusinessRuleException("You cannot leave your personal organisation.");

        var member = await _db.OrgMembers
            .FirstOrDefaultAsync(m => m.OrgId == request.OrganizationId && m.UserId == userId, cancellationToken)
            .ConfigureAwait(false);

        if (member is null || !member.IsActive)
            throw new NotFoundException("You are not currently a member of this organisation.");

        if (member.Role == OrgRole.Owner)
            throw new BusinessRuleException("Owners must transfer ownership before leaving the organisation.");

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        member.IsActive = false;
        member.LeftAt = now;

        var spaceIds = await _db.Spaces.AsNoTracking()
            .Where(s => s.OrgId == request.OrganizationId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (spaceIds.Count > 0)
        {
            var spaceMemberships = await _db.SpaceMembers
                .Where(sm => sm.UserId == userId
                             && spaceIds.Contains(sm.SpaceId)
                             && sm.LeftAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var sm in spaceMemberships)
                sm.LeftAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new LeaveOrganizationResponse(request.OrganizationId);
    }
}
