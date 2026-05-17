using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// When a finance <see cref="SpaceModule"/> row is first activated (or re-enabled), inserts the
/// current UTC calendar month row into <c>FIN_MONTHLY_PERIODS</c> if it does not yet exist.
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

        foreach (var entry in context.ChangeTracker.Entries<SpaceModule>())
        {
            if (entry.Entity.ModuleCode != ModuleCode.Finance || !entry.Entity.IsEnabled)
                continue;

            var becameEnabled = entry.State == EntityState.Added
                || (entry.State == EntityState.Modified
                    && entry.Property(nameof(SpaceModule.IsEnabled)).IsModified
                    && entry.Entity.IsEnabled
                    && !(bool)(entry.Property(nameof(SpaceModule.IsEnabled)).OriginalValue ?? false));

            if (!becameEnabled) continue;

            var utc = DateTime.UtcNow;
            var year = utc.Year;
            var month = utc.Month;
            var smoduleId = entry.Entity.Id;

            if (HasPeriodTrackedOrPersisted(context, smoduleId, year, month, entry.State))
                continue;

            context.Add(new FinMonthlyPeriod
            {
                SmoduleId = smoduleId,
                Year = year,
                Month = month,
                TotalIncome = 0,
                TotalExpense = 0,
                Net = 0,
                Status = PeriodStatus.Open,
            });
        }
    }

    private static bool HasPeriodTrackedOrPersisted(
        DbContext context,
        Guid smoduleId,
        int year,
        int month,
        EntityState spaceModuleState)
    {
        if (context.ChangeTracker.Entries<FinMonthlyPeriod>()
            .Any(e => e.Entity.SmoduleId == smoduleId && e.Entity.Year == year && e.Entity.Month == month
                      && e.State != EntityState.Deleted))
            return true;

        if (spaceModuleState == EntityState.Added)
            return false;

        return context.Set<FinMonthlyPeriod>().AsNoTracking()
            .Any(p => p.SmoduleId == smoduleId && p.Year == year && p.Month == month);
    }
}
