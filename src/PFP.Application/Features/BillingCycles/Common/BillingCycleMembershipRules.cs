using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Eligibility rules for linking deferred transactions to billing cycles.</summary>
public static class BillingCycleMembershipRules
{
    private static readonly BillingCycleStatus[] LockedCycleStatuses =
    [
        BillingCycleStatus.Closed,
        BillingCycleStatus.Paid,
        BillingCycleStatus.Overdue,
    ];

    /// <summary>Refresh: deferred on the card within the cycle spending period.</summary>
    public static bool IsTxnInRefreshPeriod(FinTransaction txn, FinBillingCycle cycle) =>
        txn.SourceId == cycle.SourceId
        && txn.TxnDate >= cycle.PeriodStart
        && txn.TxnDate <= cycle.PeriodEnd;

    /// <summary>Manual add: deferred on the card, on or before the statement date.</summary>
    public static bool MatchesManualAddEligibility(FinTransaction txn, FinBillingCycle cycle) =>
        !txn.IsDeleted
        && txn.Type == TransactionType.Deferred
        && txn.Status == TxnStatus.New
        && txn.SourceId == cycle.SourceId
        && txn.TxnDate <= cycle.StatementDate;

    public static async Task<bool> HasActiveItemInLockedCycleAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        return await (
            from item in db.FinBillingCycleItems.AsNoTracking()
            join cycle in db.FinBillingCycles.AsNoTracking() on item.BillingCycleId equals cycle.Id
            where item.TransactionId == transactionId
                  && item.RemovedAt == null
                  && LockedCycleStatuses.Contains(cycle.Status)
            select item.Id
        ).AnyAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<FinBillingCycleItem?> GetActiveItemAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        return await db.FinBillingCycleItems
            .FirstOrDefaultAsync(
                i => i.TransactionId == transactionId && i.RemovedAt == null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> HasActiveItemOnAnyCycleAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        return await db.FinBillingCycleItems
            .AsNoTracking()
            .AnyAsync(
                i => i.TransactionId == transactionId && i.RemovedAt == null,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<bool> CanAddTransactionToOpenCycleAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        FinBillingCycle cycle,
        CancellationToken cancellationToken)
    {
        if (cycle.Status != BillingCycleStatus.Open)
            return false;

        if (!MatchesManualAddEligibility(txn, cycle))
            return false;

        return !await HasActiveItemOnAnyCycleAsync(db, txn.Id, cancellationToken).ConfigureAwait(false);
    }
}
