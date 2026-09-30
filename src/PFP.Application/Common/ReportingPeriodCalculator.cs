using PFP.Domain.Enums;

namespace PFP.Application.Common;

/// <summary>A concrete reporting window with an inclusive start and exclusive end.</summary>
public readonly record struct ReportingPeriod(DateOnly StartInclusive, DateOnly EndExclusive)
{
    public bool Contains(DateOnly date) => date >= StartInclusive && date < EndExclusive;
}

/// <summary>Central reporting-cycle date arithmetic shared by reports, budgets, and alerts.</summary>
public static class ReportingPeriodCalculator
{
    public static ReportingPeriod ForTargetMonth(
        int year,
        int month,
        int reportDay,
        MonthlyReportPeriodMode periodMode = MonthlyReportPeriodMode.LowerBoundary)
    {
        Validate(year, month, reportDay, periodMode);
        var targetMonth = new DateOnly(year, month, 1);
        var startMonth = periodMode == MonthlyReportPeriodMode.LowerBoundary
            ? targetMonth.AddMonths(-1)
            : targetMonth;
        var endMonth = startMonth.AddMonths(1);
        return new ReportingPeriod(
            DayInMonth(startMonth.Year, startMonth.Month, reportDay),
            DayInMonth(endMonth.Year, endMonth.Month, reportDay));
    }

    /// <summary>Returns the report month whose concrete window contains <paramref name="date"/>.</summary>
    public static (int Year, int Month) TargetMonthContaining(
        DateOnly date,
        int reportDay,
        MonthlyReportPeriodMode periodMode = MonthlyReportPeriodMode.LowerBoundary)
    {
        if (reportDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(reportDay), "Report day must be between 1 and 31.");
        if (!Enum.IsDefined(periodMode))
            throw new ArgumentOutOfRangeException(nameof(periodMode), "Report period mode is invalid.");

        var boundaryThisMonth = DayInMonth(date.Year, date.Month, reportDay);
        var calendarMonth = new DateOnly(date.Year, date.Month, 1);
        var target = periodMode == MonthlyReportPeriodMode.LowerBoundary
            ? date < boundaryThisMonth ? calendarMonth : calendarMonth.AddMonths(1)
            : date < boundaryThisMonth ? calendarMonth.AddMonths(-1) : calendarMonth;
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

    private static void Validate(
        int year,
        int month,
        int reportDay,
        MonthlyReportPeriodMode periodMode)
    {
        _ = new DateOnly(year, month, 1);
        if (reportDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(reportDay), "Report day must be between 1 and 31.");
        if (!Enum.IsDefined(periodMode))
            throw new ArgumentOutOfRangeException(nameof(periodMode), "Report period mode is invalid.");
    }
}
