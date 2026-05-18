using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.DeleteOrganization;

/// <summary>
/// Marks the organisation's members inactive and detaches owned spaces/modules. Personal
/// organisations are protected — those are removed by the GDPR deletion flow only.
/// </summary>
public sealed class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, DeleteOrganizationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public DeleteOrganizationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<DeleteOrganizationResponse> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (org is null)
            throw new NotFoundException("Organisation was not found.");

        if (org.IsPersonal)
            throw new BusinessRuleException("Personal organisations can only be removed via account deletion.");

        if (org.OwnerId != userId)
            throw new ForbiddenException("Only the owner can delete an organisation.");

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var members = await _db.OrgMembers
            .Where(m => m.OrgId == org.Id && m.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var m in members)
        {
            m.IsActive = false;
            m.LeftAt = now;
        }

        var spaces = await _db.Spaces
            .Where(s => s.OrgId == org.Id)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (spaces.Count > 0)
        {
            var memberships = await _db.SpaceMembers
                .Where(sm => spaces.Contains(sm.SpaceId) && sm.LeftAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var sm in memberships)
                sm.LeftAt = now;

            var modules = await _db.SpaceModules
                .Where(mod => spaces.Contains(mod.SpaceId) && mod.IsEnabled)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var mod in modules)
            {
                mod.IsEnabled = false;
                mod.DisabledAt = now;
            }
        }

        // The Organization row itself stays for audit / historical FK integrity (it is not a
        // SoftDeletableEntity). Marking every membership inactive + spaces/modules disabled is
        // enough to take the org out of every read path (filtered by IsActive / LeftAt).
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteOrganizationResponse(request.OrganizationId);
    }
}
