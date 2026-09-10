using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Budgets.Common;

namespace PFP.Application.Features.MonthlyPeriods.Common;

internal static class MonthlyReportBudgetProjector
{
    public static async Task<MonthlyReportDto> ApplyAsync(
        IApplicationDbContext db,
        MonthlyReportDto report,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var budgets = await db.FinCategoryBudgets
            .AsNoTracking()
            .Where(budget => budget.UserId == userId && budget.IsEnabled)
            .Select(budget => new BudgetRow(
                budget.CategoryId,
                budget.Category.Name,
                budget.Currency,
                budget.Amount,
                budget.TargetMode,
                budget.WarningThresholdsJson))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var groups = (report.CurrencyGroups ?? Array.Empty<MonthlyReportCurrencyGroupDto>())
            .Select(group => group with
            {
                BudgetUtilizations = BuildForCurrency(group, budgets),
            })
            .ToList();

        var primary = groups.FirstOrDefault(group =>
            string.Equals(group.Currency, report.Metadata?.Currency, StringComparison.Ordinal));

        return report with
        {
            CurrencyGroups = groups,
            BudgetUtilizations = primary?.BudgetUtilizations ?? Array.Empty<CategoryBudgetUtilizationDto>(),
        };
    }

    private static IReadOnlyList<CategoryBudgetUtilizationDto> BuildForCurrency(
        MonthlyReportCurrencyGroupDto group,
        IEnumerable<BudgetRow> budgets)
    {
        var spentByCategory = group.CategoryBreakdown
            .Where(row => row.CategoryId.HasValue)
            .ToDictionary(row => row.CategoryId!.Value, row => CurrencyUnits.FromWhole(row.Amount));

        var rows = new List<CategoryBudgetUtilizationDto>();
        foreach (var budget in budgets.Where(b => string.Equals(
                     b.Currency,
                     group.Currency,
                     StringComparison.Ordinal)))
        {
            var categoryId = budget.CategoryId;
            var amount = budget.Amount;
            var spent = spentByCategory.GetValueOrDefault(categoryId);
            var thresholds = BudgetThresholdJson.Deserialize(budget.WarningThresholdsJson);
            var evaluation = BudgetTargetRules.Evaluate(
                spent,
                amount,
                budget.TargetMode,
                thresholds);

            rows.Add(new CategoryBudgetUtilizationDto(
                categoryId,
                budget.CategoryName,
                group.Currency,
                CurrencyUnits.ToWhole(spent),
                CurrencyUnits.ToWhole(amount),
                CurrencyUnits.ToWhole(evaluation.RemainingAmount),
                evaluation.ProgressPercent,
                evaluation.Status,
                thresholds,
                budget.TargetMode));
        }

        return rows
            .OrderByDescending(row => row.UtilizationPercent)
            .ThenBy(row => row.CategoryName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private sealed record BudgetRow(
        Guid CategoryId,
        string CategoryName,
        string Currency,
        decimal Amount,
        PFP.Domain.Enums.BudgetTargetMode TargetMode,
        string WarningThresholdsJson);
}
