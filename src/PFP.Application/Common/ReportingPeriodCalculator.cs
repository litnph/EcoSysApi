namespace PFP.Application.Common;

/// <summary>A concrete reporting window with an inclusive start and exclusive end.</summary>
public readonly record struct ReportingPeriod(DateOnly StartInclusive, DateOnly EndExclusive)
{
    public bool Contains(DateOnly date) => date >= StartInclusive && date < EndExclusive;
}

/// <summary>Central reporting-cycle date arithmetic shared by reports, budgets, and alerts.</summary>
public static class ReportingPeriodCalculator
{
    public static ReportingPeriod ForTargetMonth(int year, int month, int reportDay)
    {
        Validate(year, month, reportDay);
        var targetMonth = new DateOnly(year, month, 1);
        var previousMonth = targetMonth.AddMonths(-1);
        return new ReportingPeriod(
            DayInMonth(previousMonth.Year, previousMonth.Month, reportDay),
            DayInMonth(targetMonth.Year, targetMonth.Month, reportDay));
    }

    /// <summary>Returns the report month whose concrete window contains <paramref name="date"/>.</summary>
    public static (int Year, int Month) TargetMonthContaining(DateOnly date, int reportDay)
    {
        if (reportDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(reportDay), "Report day must be between 1 and 31.");

        var boundaryThisMonth = DayInMonth(date.Year, date.Month, reportDay);
        var target = date < boundaryThisMonth
            ? new DateOnly(date.Year, date.Month, 1)
            : new DateOnly(date.Year, date.Month, 1).AddMonths(1);
        return (target.Year, target.Month);
    }

    public static DateOnly DayInMonth(int year, int month, int reportDay)
    {
        if (reportDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(reportDay), "Report day must be between 1 and 31.");
        return new DateOnly(year, month, Math.Min(reportDay, DateTime.DaysInMonth(year, month)));
    }

    /// <summary>Converts a finance-calendar midnight to its UTC instant without local-machine timezone arithmetic.</summary>
    public static DateTimeOffset ToUtcBoundary(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(FinanceBusinessCalendar.TimeZoneId);
        var utc = TimeZoneInfo.ConvertTimeToUtc(localMidnight, zone);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }

    private static void Validate(int year, int month, int reportDay)
    {
        _ = new DateOnly(year, month, 1);
        if (reportDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(reportDay), "Report day must be between 1 and 31.");
    }
}
