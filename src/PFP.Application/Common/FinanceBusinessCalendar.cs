namespace PFP.Application.Common;

/// <summary>
/// Defines the calendar used for finance posting, due-date, maturity, and reporting boundaries.
/// Audit timestamps and token expiry remain UTC instants and must not use this calendar.
/// </summary>
public static class FinanceBusinessCalendar
{
    /// <summary>The approved shared-ledger reporting time zone.</summary>
    public const string TimeZoneId = "Asia/Bangkok";

    private static readonly TimeZoneInfo BusinessTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);

    /// <summary>Gets today's finance date in the approved reporting time zone.</summary>
    public static DateOnly Today => GetDate(DateTimeOffset.UtcNow);

    /// <summary>Converts an absolute instant to its finance calendar date.</summary>
    public static DateOnly GetDate(DateTimeOffset instant)
    {
        var local = TimeZoneInfo.ConvertTime(instant, BusinessTimeZone);
        return DateOnly.FromDateTime(local.DateTime);
    }

    /// <summary>Converts a UTC database timestamp to its finance calendar date.</summary>
    public static DateOnly GetDate(DateTime utcInstant)
    {
        var normalizedUtc = utcInstant.Kind switch
        {
            DateTimeKind.Utc => utcInstant,
            DateTimeKind.Local => utcInstant.ToUniversalTime(),
            _ => DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc),
        };

        return GetDate(new DateTimeOffset(normalizedUtc));
    }
}
