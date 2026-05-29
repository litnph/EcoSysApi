using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;

public sealed class RefreshBillingCycleCommandHandler
    : IRequestHandler<RefreshBillingCycleCommand, RefreshBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RefreshBillingCycleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefreshBillingCycleResponse> Handle(
        RefreshBillingCycleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var cycle = await _db.FinBillingCycles
                .Include(c => c.Source)
                .FirstOrDefaultAsync(c => c.Id == request.CycleId, ct)
                .ConfigureAwait(false);

            if (cycle is null)
                throw new NotFoundException("Billing cycle was not found.");

            if (cycle.Status != BillingCycleStatus.Open)
                throw new BusinessRuleException("Only an open billing cycle can be refreshed.");

            var candidates = await _db.FinTransactions
                .AsNoTracking()
                .Where(t =>
                    !t.IsDeleted
                    && t.Type == TransactionType.Deferred
                    && t.Status == TxnStatus.New
                    && t.SourceId == cycle.SourceId
                    && t.TxnDate >= cycle.PeriodStart
                    && t.TxnDate <= cycle.PeriodEnd)
                .Select(t => t.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            var added = 0;
            var skipped = 0;

            foreach (var txnId in candidates)
            {
                if (await BillingCycleMembershipRules.HasActiveItemInLockedCycleAsync(
                        _db, txnId, ct).ConfigureAwait(false))
                {
                    skipped++;
                    continue;
                }

                var existing = await _db.FinBillingCycleItems
                    .FirstOrDefaultAsync(
                        i => i.TransactionId == txnId && i.BillingCycleId == cycle.Id,
                        ct)
                    .ConfigureAwait(false);

                if (existing is not null)
                {
                    if (existing.RemovedAt is not null)
                    {
                        skipped++;
                        continue;
                    }

                    skipped++;
                    continue;
                }

                var activeElsewhere = await BillingCycleMembershipRules.GetActiveItemAsync(
                    _db, txnId, ct).ConfigureAwait(false);

                if (activeElsewhere is not null)
                {
                    skipped++;
                    continue;
                }

                _db.FinBillingCycleItems.Add(new FinBillingCycleItem
                {
                    BillingCycleId = cycle.Id,
                    TransactionId = txnId,
                    InclusionSource = BillingCycleItemInclusionSource.Refresh,
                });
                added++;
            }

            cycle.LastRefreshedAt = DateTime.UtcNow;
            await BillingCycleTotals.RecalculateAsync(cycle, _db, ct).ConfigureAwait(false);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return new RefreshBillingCycleResponse(
                FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name),
                added,
                skipped);
        }, cancellationToken).ConfigureAwait(false);
    }
}
