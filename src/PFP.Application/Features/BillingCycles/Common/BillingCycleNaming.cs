namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Default Vietnamese labels for billing cycles from statement period dates.</summary>
public static class BillingCycleNaming
{
    /// <summary>
    /// Builds the default cycle title from the statement month, e.g. period 20/04–19/05 → <c>Kỳ sao kê tháng 5</c>.
    /// </summary>
    public static string BuildDefaultName(DateOnly statementDate) =>
        $"Kỳ sao kê tháng {statementDate.Month}";
}
