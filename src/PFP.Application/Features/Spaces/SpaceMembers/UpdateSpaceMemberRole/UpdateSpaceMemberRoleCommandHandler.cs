using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Spaces.SpaceMembers.Common;

namespace PFP.Application.Features.Spaces.SpaceMembers.UpdateSpaceMemberRole;

public sealed class UpdateSpaceMemberRoleCommandHandler
    : IRequestHandler<UpdateSpaceMemberRoleCommand, UpdateSpaceMemberRoleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISpaceMembershipEvaluator _membershipCache;

    public UpdateSpaceMemberRoleCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ISpaceMembershipEvaluator membershipCache)
    {
        _db = db;
        _currentUser = currentUser;
        _membershipCache = membershipCache;
    }

    /// <inheritdoc/>
    public async Task<UpdateSpaceMemberRoleResponse> Handle(
        UpdateSpaceMemberRoleCommand request,
        CancellationToken cancellationToken)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            .ConfigureAwait(false);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        await SpaceMembersAuthorization.EnsureCanManageMembershipsAsync(
                _db,
                _currentUser,
                space.OrgId,
                space.Id,
                cancellationToken)
            .ConfigureAwait(false);

        var member = await _db.SpaceMembers
            .FirstOrDefaultAsync(
                m => m.SpaceId == request.SpaceId && m.UserId == request.UserId && m.LeftAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
            throw new NotFoundException("Space membership was not found.");

        member.Role = request.NewRole;

        var descendantIds = new List<Guid>();

        var updatedInherited = 0;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (request.PropagateToChildren)
        {
            descendantIds = await _db.Spaces.AsNoTracking()
                .Where(d => d.OrgId == space.OrgId && d.Path.StartsWith(space.Path + "/"))
                .Select(d => d.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var inheritedRows = await _db.SpaceMembers
                .Where(
                    m => descendantIds.Contains(m.SpaceId)
                         && m.UserId == request.UserId
                         && m.Inherited
                         && m.LeftAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var row in inheritedRows)
            {
                row.Role = request.NewRole;
                updatedInherited++;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var affectedSpaces = new List<Guid> { space.Id };

        if (request.PropagateToChildren && descendantIds.Count > 0)
            affectedSpaces.AddRange(descendantIds);

        await _membershipCache.InvalidateMembershipBatchAsync(request.UserId, affectedSpaces, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateSpaceMemberRoleResponse(space.Id, request.UserId, updatedInherited);
    }
}
