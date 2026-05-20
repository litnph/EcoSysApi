using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities.Finance;
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
        IQueryable<FinBillingCycle> q = _db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source);

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
