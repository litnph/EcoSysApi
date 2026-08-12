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
    IReadOnlyList<MonthlyReportCurrencyGroupDto>? CurrencyGroups = null);

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
    MonthlyReportBillingCyclesSectionDto BillingCycles);

/// <summary>Machine-readable report semantics used to interpret and reconcile a snapshot.</summary>
public sealed record MonthlyReportMetadataDto(
    string FormulaVersion,
    string MetricBasis,
    string? Currency,
    string TimeZone,
    bool ConsolidatedTotalsAvailable = true);

/// <summary>Totals and savings rate for the report header.</summary>
public sealed record MonthlyReportSummaryDto(
    long TotalIncome,
    long TotalExpense,
    long Net,
    decimal? SavingsRatePercent);
