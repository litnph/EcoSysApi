namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Maps issuer statement-day-of-month rules onto concrete <see cref="DateOnly"/> values (month length clamping).</summary>
public static class BillingCycleCalendar
{
    /// <summary>Returns the calendar date for <paramref name="dayOfMonth"/> within the given month, clamped to the month's last day.</summary>
    public static DateOnly DayInMonth(int year, int month, int dayOfMonth)
    {
        var last = DateTime.DaysInMonth(year, month);
        var day = dayOfMonth > last ? last : dayOfMonth;
        return new DateOnly(year, month, day);
    }

    /// <summary>Resolves dates for the billing cycle whose statement falls in the current calendar month (UTC).</summary>
    public static BillingCyclePeriodDates ResolveCurrentPeriod(
        DateOnly today,
        int statementDay,
        int paymentDueDaysAfterStatement)
    {
        var statementDate = DayInMonth(today.Year, today.Month, statementDay);
        return BuildPeriodFromStatementDate(statementDate, statementDay, paymentDueDaysAfterStatement);
    }

    /// <summary>
    /// Resolves dates for a cycle by statement month, e.g. May 2026 → period 20/04–19/05 when statement day is 20.
    /// </summary>
    public static BillingCyclePeriodDates ResolveForStatementMonth(
        int statementYear,
        int statementMonth,
        int statementDay,
        int paymentDueDaysAfterStatement)
    {
        var statementDate = DayInMonth(statementYear, statementMonth, statementDay);
        return BuildPeriodFromStatementDate(statementDate, statementDay, paymentDueDaysAfterStatement);
    }

    private static BillingCyclePeriodDates BuildPeriodFromStatementDate(
        DateOnly statementDate,
        int statementDay,
        int paymentDueDaysAfterStatement)
    {
        var periodEnd = statementDate.AddDays(-1);
        var prev = statementDate.AddMonths(-1);
        var periodStart = DayInMonth(prev.Year, prev.Month, statementDay);
        var paymentDueDate = statementDate.AddDays(paymentDueDaysAfterStatement);
        return new BillingCyclePeriodDates(periodStart, periodEnd, statementDate, paymentDueDate);
    }
}

/// <summary>Computed billing-cycle date range for a statement month.</summary>
public sealed record BillingCyclePeriodDates(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly StatementDate,
    DateOnly PaymentDueDate);
