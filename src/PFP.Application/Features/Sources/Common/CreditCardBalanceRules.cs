using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>
/// Derives outstanding credit-card debt from unsettled ordinary charges and unpaid installment
/// schedule lines. Paid statement items are excluded as settled, while a plan's original charge is
/// replaced by its remaining schedule so legacy inferred payments are never deducted twice.
/// </summary>
public static class CreditCardBalanceRules
{
    /// <summary>Recomputes outstanding debt for a credit-card source.</summary>
    public static async Task<decimal> ComputeOutstandingAsync(
        IApplicationDbContext db,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var installmentOriginIds = db.FinInstallmentPlans.AsNoTracking()
            .Where(plan => plan.SourceId == sourceId)
            .Select(plan => plan.OriginalTxnId);

        var paidStatementTransactionIds =
            from item in db.FinBillingCycleItems.AsNoTracking()
            join cycle in db.FinBillingCycles.AsNoTracking()
                on item.BillingCycleId equals cycle.Id
            where cycle.SourceId == sourceId
                  && cycle.Status == BillingCycleStatus.Paid
                  && item.RemovedAt == null
            select item.TransactionId;

        // Card purchases normalize to Deferred. Keep explicit refunds and adjustments, but ignore
        // legacy general Income/Transfer repair legs: their effect is already represented by the
        // statement and installment state projected below.
        var ordinaryLegs = await db.FinTransactions.AsNoTracking()
            .Where(t => t.SourceId == sourceId
                        && t.Status != TxnStatus.Cancelled
                        && t.Type != TransactionType.Reversal
                        && !t.IsDeleted
                        && (t.Type == TransactionType.Deferred
                            || t.Type == TransactionType.Direct
                            || t.Type == TransactionType.BalanceAdjustment
                            || (t.Type == TransactionType.Income
                                && t.Purpose == TransactionPurpose.Refund))
                        && !installmentOriginIds.Contains(t.Id)
                        && !paidStatementTransactionIds.Contains(t.Id))
            .Select(t => new { t.Type, t.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var running = 0m;
        foreach (var leg in ordinaryLegs)
            running = ApplyChargeLeg(running, leg.Type, leg.Amount);

        var unpaidInstallments = await (
            from plan in db.FinInstallmentPlans.AsNoTracking()
            where plan.SourceId == sourceId && plan.Status != InstallmentStatus.Cancelled
            from pay in plan.Pays
            where pay.Status != InstallmentPayStatus.Paid
            select pay.Amount
        ).SumAsync(cancellationToken)
            .ConfigureAwait(false);
        running += unpaidInstallments;

        // A partially paid statement still owns its unsettled items, so only its posted payment is
        // deducted. Fully paid statements were already removed through paidStatementTransactionIds,
        // and their installment lines already carry Paid status.
        var partialStatementPayments = await db.FinBillingCycles.AsNoTracking()
            .Where(cycle => cycle.SourceId == sourceId
                            && cycle.Status != BillingCycleStatus.Paid)
            .SumAsync(cycle => cycle.PaidAmount, cancellationToken)
            .ConfigureAwait(false);
        running -= partialStatementPayments;

        return decimal.Round(Math.Max(0m, running), 2, MidpointRounding.ToEven);
    }

    /// <summary>Utilization percent from outstanding debt and credit limit (whole units).</summary>
    public static decimal? UtilizationPercent(long outstandingDebt, long? creditLimitWhole)
    {
        if (creditLimitWhole is not > 0)
            return null;

        var debt = Math.Max(0L, outstandingDebt);
        return Math.Round(debt * 100m / creditLimitWhole.Value, 1, MidpointRounding.AwayFromZero);
    }

    private static decimal ApplyChargeLeg(decimal running, TransactionType type, decimal amount) =>
        type switch
        {
            // Card charges are persisted as deferred; treat stray direct rows as charges too.
            TransactionType.Deferred => running + amount,
            TransactionType.Direct => running + amount,
            TransactionType.Transfer => running + amount,
            TransactionType.Income => running - amount,
            TransactionType.BalanceAdjustment => running + amount,
            _ => running,
        };
}
