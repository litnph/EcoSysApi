using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Full finance report for one calendar month.</summary>
public sealed record MonthlyReportDto(
    MonthlyReportSummaryDto Summary,
    IReadOnlyList<MonthCategoryBreakdownItemDto> CategoryBreakdown,
    IReadOnlyList<MonthSourceBreakdownItemDto> SourceBreakdown,
    IReadOnlyList<MonthlyReportTopTransactionDto> TopTransactions,
    IReadOnlyList<DailyCashflowDto> DailyBreakdown,
    MonthOverMonthComparisonDto ComparisonWithPreviousMonth,
    MonthlyReportDirectExpenseSectionDto DirectExpenses,
    MonthlyReportBillingCyclesSectionDto BillingCycles,
    MonthlyReportMetadataDto? Metadata = null,
    IReadOnlyList<MonthlyReportCurrencyGroupDto>? CurrencyGroups = null,
    IReadOnlyList<CategoryBudgetUtilizationDto>? BudgetUtilizations = null);

/// <summary>One internally consistent report projection for a single ISO-4217 currency.</summary>
public sealed record MonthlyReportCurrencyGroupDto(
    string Currency,
    MonthlyReportSummaryDto Summary,
    IReadOnlyList<MonthCategoryBreakdownItemDto> CategoryBreakdown,
    IReadOnlyList<MonthSourceBreakdownItemDto> SourceBreakdown,
    IReadOnlyList<MonthlyReportTopTransactionDto> TopTransactions,
    IReadOnlyList<DailyCashflowDto> DailyBreakdown,
    MonthOverMonthComparisonDto ComparisonWithPreviousMonth,
    MonthlyReportDirectExpenseSectionDto DirectExpenses,
    MonthlyReportBillingCyclesSectionDto BillingCycles,
    IReadOnlyList<CategoryBudgetUtilizationDto>? BudgetUtilizations = null);

/// <summary>Machine-readable report semantics used to interpret and reconcile a snapshot.</summary>
public sealed record MonthlyReportMetadataDto(
    string FormulaVersion,
    string MetricBasis,
    string? Currency,
    string TimeZone,
    bool ConsolidatedTotalsAvailable = true,
    DateOnly? ReportingPeriodStart = null,
    DateOnly? ReportingPeriodEnd = null,
    int MonthlyReportDay = 1);

/// <summary>Spent-versus-budget projection for one category in one currency.</summary>
public sealed record CategoryBudgetUtilizationDto(
    Guid CategoryId,
    string CategoryName,
    string Currency,
    long SpentAmount,
    long BudgetAmount,
    long RemainingAmount,
    decimal UtilizationPercent,
    string Status,
    IReadOnlyList<decimal> WarningThresholds,
    BudgetTargetMode TargetMode = BudgetTargetMode.Maximum);

/// <summary>Totals and savings rate for the report header.</summary>
public sealed record MonthlyReportSummaryDto(
    long TotalIncome,
    long TotalExpense,
    long Net,
    decimal? SavingsRatePercent);
