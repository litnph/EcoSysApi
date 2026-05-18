using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleDetail;

/// <summary>Loads a billing cycle and every FIN_TRANSACTIONS row linked via <c>billing_cycle_id</c>.</summary>
public sealed class GetBillingCycleDetailQueryHandler : IRequestHandler<GetBillingCycleDetailQuery, GetBillingCycleDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetBillingCycleDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Projects the cycle DTO and transaction lines sorted by <c>txn_date</c> descending.</summary>
    /// <inheritdoc />
    public async Task<GetBillingCycleDetailResponse> Handle(GetBillingCycleDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var cycle = await _db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source)
            .ThenInclude(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(bc => bc.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(cycle.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this billing cycle.");

        if (_currentUser.CurrentOrgId is { } orgId && cycle.Source.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this billing cycle.");

        var cycleDto = FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name);

        var txnRows = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .Where(t => t.BillingCycleId == request.CycleId)
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var txns = txnRows
            .Select(t => FinBillingCycleDtoMapper.ToTransactionDto(t, t.Source.Name, t.Category?.Name))
            .ToList();

        return new GetBillingCycleDetailResponse(new FinBillingCycleDetailDto(cycleDto, txns));
    }
}
