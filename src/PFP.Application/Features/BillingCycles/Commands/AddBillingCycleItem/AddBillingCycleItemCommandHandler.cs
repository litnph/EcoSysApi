using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.AddBillingCycleItem;

public sealed class AddBillingCycleItemCommandHandler
    : IRequestHandler<AddBillingCycleItemCommand, RefreshBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddBillingCycleItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefreshBillingCycleResponse> Handle(
        AddBillingCycleItemCommand request,
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
                throw new BusinessRuleException("Only an open billing cycle can be edited.");

            var txn = await _db.FinTransactions
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId, ct)
                .ConfigureAwait(false);

            if (txn is null || txn.IsDeleted)
                throw new NotFoundException("Transaction was not found.");

            if (!await BillingCycleMembershipRules.CanAddTransactionToOpenCycleAsync(
                    _db, txn, cycle, ct).ConfigureAwait(false))
            {
                throw new BusinessRuleException(
                    "Transaction cannot be added: it must be a new deferred transaction on this card, dated on or before the statement date, and not on any billing cycle.");
            }

            var existing = await _db.FinBillingCycleItems
                .FirstOrDefaultAsync(
                    i => i.TransactionId == txn.Id && i.BillingCycleId == cycle.Id,
                    ct)
                .ConfigureAwait(false);

            if (existing is { RemovedAt: null })
                throw new BusinessRuleException("Transaction is already on this statement.");

            if (existing is not null)
            {
                existing.RemovedAt = null;
                existing.InclusionSource = BillingCycleItemInclusionSource.ManualAdd;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.FinBillingCycleItems.Add(new FinBillingCycleItem
                {
                    BillingCycleId = cycle.Id,
                    TransactionId = txn.Id,
                    InclusionSource = BillingCycleItemInclusionSource.ManualAdd,
                });
            }

            await BillingCycleTotals.RecalculateAsync(cycle, _db, ct).ConfigureAwait(false);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return new RefreshBillingCycleResponse(
                FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name),
                AddedCount: 1,
                SkippedCount: 0);
        }, cancellationToken).ConfigureAwait(false);
    }
}
