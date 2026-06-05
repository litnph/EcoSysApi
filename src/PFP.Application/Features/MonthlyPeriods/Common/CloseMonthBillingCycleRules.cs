using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>
/// Billing-cycle gates for <see cref="MonthlyPeriods.CloseMonth.CloseMonthCommand"/>.
/// Mirrors FE <c>billingCyclesBlockingCloseMonth</c>.
/// </summary>
internal static class CloseMonthBillingCycleRules
{
    /// <summary>
    /// Cycles whose <see cref="FinBillingCycle.PeriodEnd"/> falls in the calendar month and are not
    /// <see cref="BillingCycleStatus.Closed"/> or <see cref="BillingCycleStatus.Paid"/>.
    /// </summary>
    public static async Task<IReadOnlyList<FinBillingCycle>> GetBlockingCyclesAsync(
        IApplicationDbContext db,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        return await db.FinBillingCycles
            .AsNoTracking()
            .Include(bc => bc.Source)
            .Where(bc =>
                bc.PeriodEnd.Year == year
                && bc.PeriodEnd.Month == month
                && bc.Status != BillingCycleStatus.Closed
                && bc.Status != BillingCycleStatus.Paid)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
