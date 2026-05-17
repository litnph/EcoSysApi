namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Maps issuer statement-day-of-month rules onto concrete <see cref="DateOnly"/> values (month length clamping).</summary>
internal static class BillingCycleCalendar
{
    /// <summary>Returns the calendar date for <paramref name="dayOfMonth"/> within the given month, clamped to the month's last day.</summary>
    internal static DateOnly DayInMonth(int year, int month, int dayOfMonth)
    {
        var last = DateTime.DaysInMonth(year, month);
        var day = dayOfMonth > last ? last : dayOfMonth;
        return new DateOnly(year, month, day);
    }
}
