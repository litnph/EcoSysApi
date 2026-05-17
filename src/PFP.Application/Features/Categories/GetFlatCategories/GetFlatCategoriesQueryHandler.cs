using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Categories.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Categories.GetFlatCategories;

/// <summary>Projects categories as a flat list for dropdowns.</summary>
public sealed class GetFlatCategoriesQueryHandler : IRequestHandler<GetFlatCategoriesQuery, GetFlatCategoriesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetFlatCategoriesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Loads categories ordered for UI pickers.</summary>
    /// <inheritdoc cref="IRequestHandler{GetFlatCategoriesQuery, GetFlatCategoriesResponse}.Handle" />
    public async Task<GetFlatCategoriesResponse> Handle(GetFlatCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read finance categories for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var rows = await _db.FinCategories
            .AsNoTracking()
            .Where(c => c.SmoduleId == request.SmoduleId && c.Kind == request.Kind)
            .OrderBy(c => c.Depth)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows.Select(CategoryDtoMapper.ToFlatDto).ToList();
        return new GetFlatCategoriesResponse(items);
    }
}
