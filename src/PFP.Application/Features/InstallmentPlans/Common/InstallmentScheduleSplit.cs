namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>Splits a principal into per-period amounts: majority periods use standard rounding; last period takes the remainder.</summary>
public static class InstallmentScheduleSplit
{
    /// <summary>
    /// Each of the first <paramref name="months"/> - 1 periods uses half-away-from-zero rounding of the average;
    /// the last period receives whatever principal remains so the parts sum to <paramref name="totalAmount"/>.
    /// </summary>
    public static (decimal MonthlyShare, decimal LastShare) Split(decimal totalAmount, int months)
    {
        if (months < 1)
            throw new ArgumentOutOfRangeException(nameof(months));

        if (months == 1)
            return (totalAmount, totalAmount);

        var monthlyShare = decimal.Round(totalAmount / months, 0, MidpointRounding.AwayFromZero);
        var lastShare = totalAmount - monthlyShare * (months - 1);
        if (lastShare < 0)
            throw new InvalidOperationException("Installment split produced a negative final period amount.");

        return (monthlyShare, lastShare);
    }
}
