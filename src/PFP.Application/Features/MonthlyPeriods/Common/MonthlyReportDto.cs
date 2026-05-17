namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Full finance report for one calendar month.</summary>
public sealed record MonthlyReportDto(
    MonthlyReportSummaryDto Summary,
    IReadOnlyList<MonthCategoryBreakdownItemDto> CategoryBreakdown,
    IReadOnlyList<MonthSourceBreakdownItemDto> SourceBreakdown,
    IReadOnlyList<MonthlyReportTopTransactionDto> TopTransactions,
    IReadOnlyList<DailyCashflowDto> DailyBreakdown,
    MonthOverMonthComparisonDto ComparisonWithPreviousMonth);

/// <summary>Totals and savings rate for the report header.</summary>
public sealed record MonthlyReportSummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Net,
    decimal? SavingsRatePercent);
