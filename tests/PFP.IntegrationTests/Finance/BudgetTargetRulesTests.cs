using PFP.Application.Features.Budgets.Common;
using PFP.Application.Features.Budgets.UpsertCategoryBudget;
using PFP.Domain.Enums;
using Xunit;

namespace PFP.IntegrationTests.Finance;

public sealed class BudgetTargetRulesTests
{
    public static IEnumerable<object[]> ComparisonCases()
    {
        yield return new object[] { BudgetTargetMode.Maximum, 4_000_000m, 5_000_000m, 80m, "withinBudget", true, 1_000_000m };
        yield return new object[] { BudgetTargetMode.Maximum, 5_000_000m, 5_000_000m, 100m, "reached", true, 0m };
        yield return new object[] { BudgetTargetMode.Maximum, 6_000_000m, 5_000_000m, 120m, "exceeded", false, -1_000_000m };
        yield return new object[] { BudgetTargetMode.Minimum, 1_500_000m, 2_000_000m, 75m, "belowTarget", false, 500_000m };
        yield return new object[] { BudgetTargetMode.Minimum, 2_000_000m, 2_000_000m, 100m, "targetAchieved", true, 0m };
        yield return new object[] { BudgetTargetMode.Minimum, 2_500_000m, 2_000_000m, 125m, "targetExceeded", true, 0m };
    }

    [Theory]
    [MemberData(nameof(ComparisonCases))]
    public void Evaluation_respects_explicit_comparison_mode(
        BudgetTargetMode mode,
        decimal actual,
        decimal target,
        decimal expectedProgress,
        string expectedStatus,
        bool expectedAcceptable,
        decimal expectedRemaining)
    {
        var result = BudgetTargetRules.Evaluate(actual, target, mode);

        Assert.Equal(expectedProgress, result.ProgressPercent);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedAcceptable, result.IsAcceptable);
        Assert.Equal(expectedRemaining, result.RemainingAmount);
    }

    [Theory]
    [InlineData(BudgetTargetMode.Maximum, "withinBudget", true)]
    [InlineData(BudgetTargetMode.Minimum, "belowTarget", false)]
    public void Zero_actual_uses_mode_specific_status(
        BudgetTargetMode mode,
        string expectedStatus,
        bool expectedAcceptable)
    {
        var result = BudgetTargetRules.Evaluate(0m, 2_000_000m, mode);

        Assert.Equal(0m, result.ProgressPercent);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedAcceptable, result.IsAcceptable);
    }

    [Fact]
    public void Invalid_comparison_mode_is_rejected_by_command_validation()
    {
        var command = new UpsertCategoryBudgetCommand(
            Guid.NewGuid(),
            true,
            2_000_000,
            "VND",
            0,
            Array.Empty<decimal>(),
            (BudgetTargetMode)999);

        var result = new UpsertCategoryBudgetCommandValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.TargetMode));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_targets_are_rejected(decimal target)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BudgetTargetRules.Evaluate(0m, target, BudgetTargetMode.Maximum));
    }

    [Fact]
    public void Large_amounts_keep_decimal_progress_precision()
    {
        var result = BudgetTargetRules.Evaluate(
            8_100_000_000_000_000m,
            9_000_000_000_000_000m,
            BudgetTargetMode.Minimum);

        Assert.Equal(90m, result.ProgressPercent);
        Assert.Equal(900_000_000_000_000m, result.RemainingAmount);
    }
}
