using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// When the first <see cref="FinSource"/> is created, ensures the current UTC calendar month exists in
/// <c>FIN_MONTHLY_PERIODS</c>.
/// </summary>
public sealed class FinanceMonthlyPeriodSeedInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Seed(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Seed(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Seed(DbContext? context)
    {
        if (context is null) return;

        var addedSources = context.ChangeTracker.Entries<FinSource>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        if (addedSources.Count == 0)
            return;

        var utc = DateTime.UtcNow;
        var year = utc.Year;
        var month = utc.Month;

        if (HasPeriodTrackedOrPersisted(context, year, month))
            return;

        context.Add(new FinMonthlyPeriod
        {
            Year = year,
            Month = month,
            TotalIncome = 0,
            TotalExpense = 0,
            Net = 0,
            Status = PeriodStatus.Open,
        });
    }

    private static bool HasPeriodTrackedOrPersisted(DbContext context, int year, int month)
    {
        if (context.ChangeTracker.Entries<FinMonthlyPeriod>()
            .Any(e => e.Entity.Year == year && e.Entity.Month == month && e.State != EntityState.Deleted))
            return true;

        return context.Set<FinMonthlyPeriod>().AsNoTracking()
            .Any(p => p.Year == year && p.Month == month);
    }
}
