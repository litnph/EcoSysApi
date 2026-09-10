using PFP.Domain.Enums;

namespace PFP.Application.Features.Budgets.Common;

public sealed record CategoryBudgetDto(
    Guid CategoryId,
    string CategoryName,
    string Currency,
    long BudgetAmount,
    bool IsEnabled,
    int WarningCount,
    IReadOnlyList<decimal> WarningThresholds,
    long SpentAmount,
    long RemainingAmount,
    decimal UtilizationPercent,
    string Status,
    int ConfigurationVersion,
    BudgetTargetMode TargetMode = BudgetTargetMode.Maximum);

public sealed record CategoryBudgetsDto(
    int Year,
    int Month,
    int MonthlyReportDay,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    IReadOnlyList<CategoryBudgetDto> Items);
