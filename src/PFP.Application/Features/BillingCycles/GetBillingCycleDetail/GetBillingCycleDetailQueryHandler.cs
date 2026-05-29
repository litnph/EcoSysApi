using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleDetail;

/// <summary>Loads a billing cycle and active statement lines from <c>fin_billing_cycle_items</c>.</summary>
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
            .FirstOrDefaultAsync(bc => bc.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        var cycleDto = FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name);

        var items = await _db.FinBillingCycleItems
            .AsNoTracking()
            .Where(i => i.BillingCycleId == request.CycleId && i.RemovedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var txnIds = items.Select(i => i.TransactionId).ToList();
        var inclusionByTxn = items.ToDictionary(i => i.TransactionId, i => i.InclusionSource);

        var txnRows = txnIds.Count == 0
            ? []
            : await _db.FinTransactions
                .AsNoTracking()
                .Include(t => t.Source)
                .Include(t => t.Category)
                .Where(t => txnIds.Contains(t.Id) && !t.IsDeleted)
                .OrderByDescending(t => t.TxnDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var txns = txnRows
            .Select(t => FinBillingCycleDtoMapper.ToTransactionDto(
                t,
                t.Source.Name,
                t.Category?.Name,
                inclusionByTxn[t.Id]))
            .ToList();

        var installmentDues = await BillingCycleInstallmentRules
            .LoadDueDtosAsync(_db, cycle, cancellationToken)
            .ConfigureAwait(false);

        return new GetBillingCycleDetailResponse(
            new FinBillingCycleDetailDto(cycleDto, txns, installmentDues));
    }
}
