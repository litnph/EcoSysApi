using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>
/// Derives outstanding credit-card debt from charges, statement payments, and paid installment
/// schedule lines. The installment status is the payment source of truth, including legacy paid
/// lines that do not have a linked transaction. Installment amounts already included in a paid
/// statement are de-duplicated so the same repayment is not applied twice.
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

        var billingCycles = await db.FinBillingCycles.AsNoTracking()
            .Where(c => c.SourceId == sourceId)
            .Select(c => new
            {
                c.Id,
                c.StatementDate,
                c.TotalAmount,
                c.PaidAmount,
                c.Status,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var billingPaid = billingCycles.Sum(c => c.PaidAmount);
        running -= billingPaid;

        var paidInstallments = await (
            from plan in db.FinInstallmentPlans.AsNoTracking()
            where plan.SourceId == sourceId
            from pay in plan.Pays
            where pay.Status == InstallmentPayStatus.Paid
            select new { pay.StatementDate, pay.Amount }
        ).ToListAsync(cancellationToken).ConfigureAwait(false);

        var statementItemTotals = await (
            from cycle in db.FinBillingCycles.AsNoTracking()
            where cycle.SourceId == sourceId
            join item in db.FinBillingCycleItems.AsNoTracking()
                on cycle.Id equals item.BillingCycleId
            join txn in db.FinTransactions.AsNoTracking()
                on item.TransactionId equals txn.Id
            where item.RemovedAt == null && !txn.IsDeleted
            group txn by cycle.Id
            into grouped
            select new
            {
                CycleId = grouped.Key,
                Total = grouped.Sum(txn => txn.Amount),
            }
        ).ToDictionaryAsync(row => row.CycleId, row => row.Total, cancellationToken)
            .ConfigureAwait(false);

        var paidInstallmentTotal = paidInstallments.Sum(pay => pay.Amount);

        // A fully paid statement already reduces the card by PaidAmount. Its installment portion
        // is TotalAmount minus ordinary transaction items, so add that overlap back before
        // applying every Paid schedule line. This also supports legacy rows whose TxnId is null.
        var statementCoveredInstallmentTotal = paidInstallments
            .GroupBy(pay => new { pay.StatementDate.Year, pay.StatementDate.Month })
            .Sum(monthPays =>
            {
                var paidScheduleAmount = monthPays.Sum(pay => pay.Amount);
                var paidStatementInstallmentAmount = billingCycles
                    .Where(cycle => cycle.Status == BillingCycleStatus.Paid
                                    && cycle.StatementDate.Year == monthPays.Key.Year
                                    && cycle.StatementDate.Month == monthPays.Key.Month)
                    .Sum(cycle => Math.Max(
                        0m,
                        cycle.TotalAmount
                        - statementItemTotals.GetValueOrDefault(cycle.Id, 0m)));

                return Math.Min(paidScheduleAmount, paidStatementInstallmentAmount);
            });

        running -= paidInstallmentTotal - statementCoveredInstallmentTotal;

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
