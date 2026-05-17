using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Spaces.SpaceMembers.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Spaces.SpaceMembers.InviteSpaceMember;

public sealed class InviteSpaceMemberCommandHandler : IRequestHandler<InviteSpaceMemberCommand, InviteSpaceMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISpaceMembershipEvaluator _membershipCache;

    public InviteSpaceMemberCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ISpaceMembershipEvaluator membershipCache)
    {
        _db = db;
        _currentUser = currentUser;
        _membershipCache = membershipCache;
    }

    /// <inheritdoc/>
    public async Task<InviteSpaceMemberResponse> Handle(InviteSpaceMemberCommand request, CancellationToken cancellationToken)
    {
        var space = await _db.Spaces.FirstOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            .ConfigureAwait(false);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        await SpaceMembersAuthorization
            .EnsureCanManageMembershipsAsync(_db, _currentUser, space.OrgId, space.Id, cancellationToken)
            .ConfigureAwait(false);

        var inviteeAlready = await _db.SpaceMembers
            .AnyAsync(m => m.SpaceId == space.Id && m.UserId == request.UserId && m.LeftAt == null, cancellationToken)
            .ConfigureAwait(false);

        if (inviteeAlready)
            throw new BusinessRuleException("This user already has membership in the space.");

        var inviteeOrg = await _db.OrgMembers.AnyAsync(
                m => m.OrgId == space.OrgId && m.UserId == request.UserId && m.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (!inviteeOrg)
            throw new BusinessRuleException("The invited user must be an active organisation member.");

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var invitedBy = _currentUser.UserId;

        var direct = new SpaceMember
        {
            SpaceId = space.Id,
            UserId = request.UserId,
            Role = request.Role,
            Inherited = false,
            InheritedFromSpaceId = null,
            InvitedBy = invitedBy,
            JoinedAt = now,
        };

        _db.SpaceMembers.Add(direct);

        var descendantIds = await _db.Spaces.AsNoTracking()
            .Where(d => d.OrgId == space.OrgId && d.Path.StartsWith(space.Path + "/"))
            .Select(d => d.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var occupiedDescendants = descendantIds.Count == 0
            ? null
            : await _db.SpaceMembers.AsNoTracking()
                .Where(
                    m => m.UserId == request.UserId
                         && descendantIds.Contains(m.SpaceId)
                         && m.LeftAt == null)
                .Select(m => m.SpaceId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var occupiedSet = occupiedDescendants is null ? null : occupiedDescendants.ToHashSet();

        var createdInherited = 0;

        foreach (var descId in descendantIds)
        {
            if (occupiedSet is not null && occupiedSet.Contains(descId))
                continue;

            _db.SpaceMembers.Add(new SpaceMember
            {
                SpaceId = descId,
                UserId = request.UserId,
                Role = request.Role,
                Inherited = true,
                InheritedFromSpaceId = space.Id,
                InvitedBy = null,
                JoinedAt = now,
            });
            createdInherited++;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        List<Guid> affectedIds = new(descendantIds.Count + 1) { space.Id };

        affectedIds.AddRange(descendantIds);

        await _membershipCache.InvalidateMembershipBatchAsync(request.UserId, affectedIds, cancellationToken)
            .ConfigureAwait(false);

        return new InviteSpaceMemberResponse(space.Id, request.UserId, request.Role, createdInherited);
    }
}
