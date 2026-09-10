using PFP.Domain.Enums;

namespace PFP.Application.Features.Budgets.Common;

/// <summary>Canonical actual-versus-target calculations for category budgets.</summary>
public static class BudgetTargetRules
{
    public const string WithinBudgetStatus = "withinBudget";
    public const string NearLimitStatus = "nearLimit";
    public const string LimitReachedStatus = "reached";
    public const string LimitExceededStatus = "exceeded";
    public const string BelowTargetStatus = "belowTarget";
    public const string TargetAchievedStatus = "targetAchieved";
    public const string TargetExceededStatus = "targetExceeded";

    public static BudgetTargetEvaluation Evaluate(
        decimal actual,
        decimal target,
        BudgetTargetMode mode,
        IReadOnlyList<decimal>? warningThresholds = null)
    {
        if (target <= 0m)
            throw new ArgumentOutOfRangeException(nameof(target), "Target must be greater than zero.");
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode), "Unsupported budget target mode.");

        var progress = decimal.Round(
            actual / target * 100m,
            2,
            MidpointRounding.AwayFromZero);

        if (mode == BudgetTargetMode.Minimum)
        {
            return new BudgetTargetEvaluation(
                progress,
                Math.Max(target - actual, 0m),
                progress > 100m
                    ? TargetExceededStatus
                    : progress == 100m
                        ? TargetAchievedStatus
                        : BelowTargetStatus,
                actual >= target);
        }

        var thresholds = warningThresholds ?? Array.Empty<decimal>();
        return new BudgetTargetEvaluation(
            progress,
            target - actual,
            progress > 100m
                ? LimitExceededStatus
                : progress == 100m
                    ? LimitReachedStatus
                    : thresholds.Count > 0 && progress >= thresholds[^1]
                        ? NearLimitStatus
                        : WithinBudgetStatus,
            actual <= target);
    }
}

public sealed record BudgetTargetEvaluation(
    decimal ProgressPercent,
    decimal RemainingAmount,
    string Status,
    bool IsAcceptable);
