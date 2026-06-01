using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>
/// Derives outstanding credit-card debt from deferred charges, statement payments,
/// and installment backfill adjustments (which are not stored as card-side transactions).
/// </summary>
public static class CreditCardBalanceRules
{
    /// <summary>Recomputes outstanding debt for a credit-card source.</summary>
    public static async Task<decimal> ComputeOutstandingAsync(
        IApplicationDbContext db,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var legs = await db.FinTransactions.AsNoTracking()
            .Where(t => t.SourceId == sourceId
                        && t.Status != TxnStatus.Cancelled
                        && t.Type != TransactionType.Reversal
                        && !t.IsDeleted)
            .Select(t => new { t.Type, t.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var running = 0m;
        foreach (var leg in legs)
            running = ApplyChargeLeg(running, leg.Type, leg.Amount);

        var billingPaid = await db.FinBillingCycles.AsNoTracking()
            .Where(c => c.SourceId == sourceId)
            .Select(c => c.PaidAmount)
            .SumAsync(cancellationToken)
            .ConfigureAwait(false);

        running -= billingPaid;

        var backfillPaid = await (
            from plan in db.FinInstallmentPlans.AsNoTracking()
            where plan.SourceId == sourceId && plan.Status != InstallmentStatus.Cancelled
            from pay in plan.Pays
            where pay.Status == InstallmentPayStatus.Paid && pay.TxnId == null
            select pay.Amount
        ).SumAsync(cancellationToken).ConfigureAwait(false);

        running -= backfillPaid;

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
