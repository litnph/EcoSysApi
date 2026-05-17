using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.MoveSpace;

/// <summary>Rewrites path/depth for the moved subtree in a single database transaction.</summary>
public sealed class MoveSpaceCommandHandler : IRequestHandler<MoveSpaceCommand, MoveSpaceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public MoveSpaceCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<MoveSpaceResponse> Handle(MoveSpaceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var spaceToMove = await _db.Spaces
            .FirstOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            .ConfigureAwait(false);

        if (spaceToMove is null)
            throw new NotFoundException("Space was not found.");

        if (_currentUser.CurrentOrgId is { } ctxOrgId && ctxOrgId != spaceToMove.OrgId)
            throw new UnauthorizedAppException("You cannot manage spaces outside the current organisation context.");

        var userId = _currentUser.UserId.Value;

        var canManageSpaces = await _db.OrgMembers
            .AnyAsync(
                m => m.OrgId == spaceToMove.OrgId
                     && m.UserId == userId
                     && m.IsActive
                     && (m.Role == OrgRole.Admin || m.Role == OrgRole.Owner),
                cancellationToken)
            .ConfigureAwait(false);

        if (!canManageSpaces)
            throw new UnauthorizedAppException("You do not have permission to move spaces for this organisation.");

        var oldPath = spaceToMove.Path;

        Space? newParent = null;
        if (request.NewParentId is { } newParentId)
        {
            newParent = await _db.Spaces
                .FirstOrDefaultAsync(s => s.Id == newParentId && s.OrgId == spaceToMove.OrgId, cancellationToken)
                .ConfigureAwait(false);

            if (newParent is null)
                throw new NotFoundException("New parent space was not found.");
        }

        var newPathRoot = newParent is null
            ? $"/{spaceToMove.OrgId}/{spaceToMove.Id}"
            : $"{newParent.Path}/{spaceToMove.Id}";

        var newDepthRoot = newParent?.Depth + 1 ?? 0;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var subtree = await _db.Spaces
            .Where(s => s.OrgId == spaceToMove.OrgId &&
                        (s.Path == oldPath || s.Path.StartsWith(oldPath + "/")))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var depthDelta = newDepthRoot - spaceToMove.Depth;

        foreach (var node in subtree)
        {
            if (node.Path == oldPath)
            {
                node.ParentId = request.NewParentId;
                node.Path = newPathRoot;
                node.Depth = newDepthRoot;
            }
            else
            {
                node.Path = newPathRoot + node.Path.Substring(oldPath.Length);
                node.Depth += depthDelta;
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new MoveSpaceResponse(
            spaceToMove.Id,
            spaceToMove.OrgId,
            spaceToMove.ParentId,
            spaceToMove.Path,
            spaceToMove.Depth);
    }
}
