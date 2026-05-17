using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.GetSpaceTree;

/// <summary>Loads all organisation spaces and stitches a nested hierarchy in memory.</summary>
public sealed class GetSpaceTreeQueryHandler : IRequestHandler<GetSpaceTreeQuery, GetSpaceTreeResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetSpaceTreeQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetSpaceTreeResponse> Handle(GetSpaceTreeQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (_currentUser.CurrentOrgId is { } ctxOrgId && ctxOrgId != request.OrgId)
            throw new UnauthorizedAppException("You cannot manage spaces outside the current organisation context.");

        var uid = _currentUser.UserId.Value;

        var isMember = await _db.OrgMembers.AnyAsync(
                m => m.OrgId == request.OrgId
                     && m.UserId == uid
                     && m.IsActive
                     && m.Role >= OrgRole.Member,
                cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new UnauthorizedAppException("You cannot read the spaces for this organisation.");

        var spaces = await _db.Spaces.AsNoTracking()
            .Where(s => s.OrgId == request.OrgId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var spaceIds = spaces.Select(x => x.Id).ToHashSet();

        var financeMods = await _db.SpaceModules.AsNoTracking()
            .Where(m => spaceIds.Contains(m.SpaceId) && m.ModuleCode == ModuleCode.Finance && m.IsEnabled)
            .Select(m => m.SpaceId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var financeEnabledLookup = financeMods.ToHashSet();

        var byParentId = spaces
            .GroupBy(s => s.ParentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList());

        IEnumerable<SpaceTreeDto> orderedChildren(Guid? parentKey) =>
            byParentId.TryGetValue(parentKey, out var list)
                ? list.Select(Project)
                : Enumerable.Empty<SpaceTreeDto>();

        SpaceTreeDto Project(Space s) =>
            new(
                s.Id,
                s.OrgId,
                s.Name,
                s.Type,
                s.Depth,
                s.Path,
                financeEnabledLookup.Contains(s.Id),
                s.SortOrder,
                s.ParentId,
                orderedChildren(s.Id).ToList());

        var rootsSpaces = spaces.Where(s => s.ParentId == null).ToList();
        var roots = rootsSpaces
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .Select(Project)
            .ToList();

        return new GetSpaceTreeResponse(roots);
    }
}
