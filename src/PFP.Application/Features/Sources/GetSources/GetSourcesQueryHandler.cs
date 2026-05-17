using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.GetSources;

/// <summary>Reads finance sources visible to the caller for the requested module.</summary>
public sealed class GetSourcesQueryHandler : IRequestHandler<GetSourcesQuery, GetSourcesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetSourcesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns all sources for the module ordered by <see cref="Domain.Entities.FinSource.SortOrder"/> then name.</summary>
    /// <inheritdoc cref="IRequestHandler{GetSourcesQuery, GetSourcesResponse}.Handle" />
    public async Task<GetSourcesResponse> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read finance sources for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var rows = await _db.FinSources
            .AsNoTracking()
            .Where(s => s.SmoduleId == request.SmoduleId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.Select(FinSourceDtoMapper.ToDto).ToList();
        return new GetSourcesResponse(dtos);
    }
}
