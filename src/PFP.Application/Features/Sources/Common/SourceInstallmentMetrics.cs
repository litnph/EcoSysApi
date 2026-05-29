using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.Common;

/// <summary>Aggregates unpaid installment amounts per credit-card source.</summary>
public static class SourceInstallmentMetrics
{
    /// <summary>
    /// Returns the sum of unpaid installment pay rows for active plans, keyed by source id.
    /// </summary>
    public static async Task<IReadOnlyDictionary<Guid, long>> GetRemainingBySourceIdAsync(
        IApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var rows = await db.FinInstallmentPlans
            .AsNoTracking()
            .Where(p => p.Status == InstallmentStatus.Active)
            .SelectMany(
                p => p.Pays.Where(pay => pay.Status != InstallmentPayStatus.Paid),
                (p, pay) => new { p.SourceId, pay.Amount })
            .GroupBy(x => x.SourceId)
            .Select(g => new { SourceId = g.Key, Remaining = g.Sum(x => x.Amount) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.ToDictionary(
            r => r.SourceId,
            r => CurrencyUnits.ToWhole(r.Remaining));
    }

    /// <summary>
    /// Returns unpaid installment amount for one source (0 when none).
    /// </summary>
    public static async Task<long> GetRemainingForSourceAsync(
        IApplicationDbContext db,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var remaining = await db.FinInstallmentPlans
            .AsNoTracking()
            .Where(p => p.Status == InstallmentStatus.Active && p.SourceId == sourceId)
            .SelectMany(
                p => p.Pays.Where(pay => pay.Status != InstallmentPayStatus.Paid),
                (_, pay) => pay.Amount)
            .SumAsync(cancellationToken)
            .ConfigureAwait(false);

        return CurrencyUnits.ToWhole(remaining);
    }
}
