using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Spaces.SpaceMembers.Common;

namespace PFP.Application.Features.Spaces.SpaceMembers.RemoveSpaceMember;

public sealed class RemoveSpaceMemberCommandHandler : IRequestHandler<RemoveSpaceMemberCommand, RemoveSpaceMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISpaceMembershipEvaluator _membershipCache;

    public RemoveSpaceMemberCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ISpaceMembershipEvaluator membershipCache)
    {
        _db = db;
        _currentUser = currentUser;
        _membershipCache = membershipCache;
    }

    /// <inheritdoc/>
    public async Task<RemoveSpaceMemberResponse> Handle(RemoveSpaceMemberCommand request, CancellationToken cancellationToken)
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

        var orgOwner = await _db.Organizations.AsNoTracking()
            .Where(o => o.Id == space.OrgId)
            .Select(o => o.OwnerId)
            .SingleAsync(cancellationToken)
            .ConfigureAwait(false);

        if (request.UserId == orgOwner)
            throw new BusinessRuleException("The organisation owner cannot be removed from spaces.");

        var member = await _db.SpaceMembers
            .FirstOrDefaultAsync(
                m => m.SpaceId == request.SpaceId && m.UserId == request.UserId && m.LeftAt == null,
                cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
            throw new NotFoundException("Space membership was not found.");

        var descendantIds = new List<Guid>();
        var removedInherited = 0;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        if (request.RemoveFromChildren)
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
                _db.SpaceMembers.Remove(row);
                removedInherited++;
            }
        }

        _db.SpaceMembers.Remove(member);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var affected = new List<Guid> { space.Id };

        if (request.RemoveFromChildren && descendantIds.Count > 0)
            affected.AddRange(descendantIds);

        await _membershipCache.InvalidateMembershipBatchAsync(request.UserId, affected, cancellationToken)
            .ConfigureAwait(false);

        return new RemoveSpaceMemberResponse(space.Id, request.UserId, removedInherited);
    }
}
