namespace PFP.Application.Features.Budgets.Common;

/// <summary>Canonical warning-threshold distribution and validation.</summary>
public static class BudgetThresholds
{
    public const int MaximumWarningCount = 10;

    public static IReadOnlyList<decimal> Generate(int count)
    {
        if (count is < 0 or > MaximumWarningCount)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return Array.Empty<decimal>();

        return Enumerable.Range(1, count)
            .Select(index => decimal.Round(
                index * 100m / (count + 1),
                2,
                MidpointRounding.AwayFromZero))
            .ToArray();
    }

    public static IReadOnlyList<decimal> Normalize(IEnumerable<decimal> thresholds)
    {
        var normalized = thresholds
            .Select(value => decimal.Round(value, 2, MidpointRounding.AwayFromZero))
            .OrderBy(value => value)
            .ToArray();

        if (normalized.Length > MaximumWarningCount)
            throw new ArgumentException($"At most {MaximumWarningCount} warning levels are allowed.", nameof(thresholds));
        if (normalized.Any(value => value <= 0m || value >= 100m))
            throw new ArgumentException("Warning thresholds must be greater than 0 and less than 100.", nameof(thresholds));
        if (normalized.Distinct().Count() != normalized.Length)
            throw new ArgumentException("Warning thresholds must be unique.", nameof(thresholds));

        return normalized;
    }

    public static bool IsValid(IReadOnlyCollection<decimal> thresholds, int warningCount) =>
        warningCount is >= 0 and <= MaximumWarningCount
        && thresholds.Count == warningCount
        && TryNormalize(thresholds, out var normalized)
        && normalized.SequenceEqual(thresholds);

    private static bool TryNormalize(IEnumerable<decimal> thresholds, out IReadOnlyList<decimal> normalized)
    {
        try
        {
            normalized = Normalize(thresholds);
            return true;
        }
        catch (ArgumentException)
        {
            normalized = Array.Empty<decimal>();
            return false;
        }
    }
}
