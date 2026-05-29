using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Recomputes <see cref="FinBillingCycle.TotalAmount"/> from active statement lines.</summary>
public static class BillingCycleTotals
{
    public static async Task<decimal> SumActiveItemsAsync(
        IApplicationDbContext db,
        Guid cycleId,
        CancellationToken cancellationToken)
    {
        return await (
            from item in db.FinBillingCycleItems.AsNoTracking()
            join txn in db.FinTransactions.AsNoTracking() on item.TransactionId equals txn.Id
            where item.BillingCycleId == cycleId
                  && item.RemovedAt == null
                  && !txn.IsDeleted
            select txn.Amount
        ).SumAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<decimal> SumInstallmentDuesAsync(
        IApplicationDbContext db,
        FinBillingCycle cycle,
        CancellationToken cancellationToken)
    {
        return await BillingCycleInstallmentRules
            .DuePaysQuery(db, cycle.SourceId, cycle.StatementDate)
            .SumAsync(p => p.Amount, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task RecalculateAsync(
        FinBillingCycle cycle,
        IApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var txnTotal = await SumActiveItemsAsync(db, cycle.Id, cancellationToken)
            .ConfigureAwait(false);
        var installmentTotal = await SumInstallmentDuesAsync(db, cycle, cancellationToken)
            .ConfigureAwait(false);
        cycle.TotalAmount = txnTotal + installmentTotal;
    }
}
