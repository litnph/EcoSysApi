using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Resolves billing-cycle statement month (YYYY-MM) for transactions on active cycle lines.</summary>
public static class BillingCycleStatementMonthQueries
{
    public static async Task<IReadOnlyDictionary<Guid, string>> ForTransactionsMapAsync(
        IApplicationDbContext db,
        IEnumerable<Guid> transactionIds,
        CancellationToken cancellationToken)
    {
        var ids = transactionIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        var rows = await (
            from item in db.FinBillingCycleItems.AsNoTracking()
            join cycle in db.FinBillingCycles.AsNoTracking() on item.BillingCycleId equals cycle.Id
            where item.RemovedAt == null && ids.Contains(item.TransactionId)
            select new { item.TransactionId, cycle.StatementDate }
        ).ToListAsync(cancellationToken).ConfigureAwait(false);

        return rows
            .GroupBy(r => r.TransactionId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var latest = g.Max(x => x.StatementDate);
                    return $"{latest.Year}-{latest.Month:D2}";
                });
    }
}
