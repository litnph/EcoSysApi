using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.GetBillingCycles;

/// <summary>Projects FIN_BILLING_CYCLES rows for the authenticated caller.</summary>
public sealed class GetBillingCyclesQueryHandler : IRequestHandler<GetBillingCyclesQuery, GetBillingCyclesResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetBillingCyclesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Loads cycles ordered by period start (descending).</summary>
    /// <inheritdoc />
    public async Task<GetBillingCyclesResponse> Handle(GetBillingCyclesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read billing cycles for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var q = _db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source)
            .Where(bc => bc.SmoduleId == request.SmoduleId);

        if (request.SourceId is { } sid)
            q = q.Where(bc => bc.SourceId == sid);

        if (request.Status is { } st)
            q = q.Where(bc => bc.Status == st);

        var cycles = await q
            .OrderByDescending(bc => bc.PeriodStart)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var rows = cycles.Select(bc => FinBillingCycleDtoMapper.ToDto(bc, bc.Source.Name)).ToList();

        return new GetBillingCyclesResponse(rows);
    }
}
