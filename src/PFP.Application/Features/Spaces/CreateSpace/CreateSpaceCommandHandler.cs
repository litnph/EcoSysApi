using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.CreateSpace;

/// <summary>Persists a new <see cref="Space"/> and cascades inherited memberships from its parent.</summary>
public sealed class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, CreateSpaceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ISpaceMembershipEvaluator _membershipCache;

    /// <summary>Creates the handler.</summary>
    public CreateSpaceCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        ISpaceMembershipEvaluator membershipCache)
    {
        _db = db;
        _currentUser = currentUser;
        _membershipCache = membershipCache;
    }

    /// <inheritdoc/>
    public async Task<CreateSpaceResponse> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (_currentUser.CurrentOrgId is { } ctxOrgId && ctxOrgId != request.OrgId)
            throw new UnauthorizedAppException("You cannot manage spaces outside the current organisation context.");

        var userId = _currentUser.UserId!.Value;

        var canManageSpaces = await _db.OrgMembers
            .AnyAsync(
                m => m.OrgId == request.OrgId
                     && m.UserId == userId
                     && m.IsActive
                     && (m.Role == OrgRole.Admin || m.Role == OrgRole.Owner),
                cancellationToken)
            .ConfigureAwait(false);

        if (!canManageSpaces)
            throw new UnauthorizedAppException("You do not have permission to manage spaces for this organisation.");

        Space? parent = null;
        if (request.ParentId is { } pid)
        {
            parent = await _db.Spaces
                .FirstOrDefaultAsync(s => s.Id == pid && s.OrgId == request.OrgId, cancellationToken)
                .ConfigureAwait(false);

            if (parent is null)
                throw new NotFoundException("Parent space was not found.");
        }

        var space = new Space
        {
            OrgId = request.OrgId,
            ParentId = request.ParentId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Type = request.Type,
            SortOrder = request.SortOrder ?? 0,
        };

        string path;
        int depth;

        if (parent is null)
        {
            path = $"/{request.OrgId}/{space.Id}";
            depth = 0;
        }
        else
        {
            path = $"{parent.Path}/{space.Id}";
            depth = parent.Depth + 1;
        }

        space.Path = path;
        space.Depth = depth;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        _db.Spaces.Add(space);

        if (parent is not null)
        {
            var now = DateTime.UtcNow;

            var parentMembers = await _db.SpaceMembers
                .AsNoTracking()
                .Where(m => m.SpaceId == parent.Id && m.LeftAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var pm in parentMembers)
            {
                var inherited = new SpaceMember
                {
                    SpaceId = space.Id,
                    UserId = pm.UserId,
                    Role = pm.Role,
                    Inherited = true,
                    InheritedFromSpaceId = parent!.Id,
                    InvitedBy = null,
                    JoinedAt = now,
                };
                _db.SpaceMembers.Add(inherited);
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        if (parent is not null)
        {
            var distinctInheritedUsers = await _db.SpaceMembers.AsNoTracking()
                .Where(m => m.SpaceId == space.Id && m.Inherited && m.LeftAt == null)
                .Select(m => m.UserId)
                .Distinct()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var uid in distinctInheritedUsers)
            {
                await _membershipCache.InvalidateMembershipAsync(uid, space.Id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new CreateSpaceResponse(space.Id, space.OrgId, space.ParentId, space.Path, space.Depth);
    }
}
