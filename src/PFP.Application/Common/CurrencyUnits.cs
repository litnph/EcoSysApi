namespace PFP.Application.Common;

/// <summary>
/// Whole-currency helpers for API wire format (spec §5.1 — VND returned as integer, no fractional units).
/// </summary>
public static class CurrencyUnits
{
    /// <summary>Rounds a stored decimal balance/amount to the nearest whole currency unit for API responses.</summary>
    public static long ToWhole(decimal amount) =>
        (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero);

    /// <summary>Converts an API whole-unit amount to the decimal persisted in the database.</summary>
    public static decimal FromWhole(long amount) => amount;
}
