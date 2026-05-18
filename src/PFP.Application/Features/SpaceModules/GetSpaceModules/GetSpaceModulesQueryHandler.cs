using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.SpaceModules.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.SpaceModules.GetSpaceModules;

/// <summary>Verifies space membership then returns the module activation rows.</summary>
public sealed class GetSpaceModulesQueryHandler : IRequestHandler<GetSpaceModulesQuery, GetSpaceModulesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetSpaceModulesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetSpaceModulesResponse> Handle(GetSpaceModulesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var canView = await _currentUser
            .IsSpaceMemberAsync(request.SpaceId, SpaceRole.Viewer, cancellationToken)
            .ConfigureAwait(false);
        if (!canView)
            throw new ForbiddenException("You do not have access to this space.");

        var rows = await _db.SpaceModules.AsNoTracking()
            .Where(m => m.SpaceId == request.SpaceId)
            .OrderBy(m => m.ModuleCode)
            .Select(m => new SpaceModuleDto(m.Id, m.SpaceId, m.ModuleCode, m.IsEnabled, m.Settings, m.EnabledAt, m.DisabledAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetSpaceModulesResponse(rows);
    }
}
