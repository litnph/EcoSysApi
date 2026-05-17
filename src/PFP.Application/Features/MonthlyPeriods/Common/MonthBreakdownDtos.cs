using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>One slice of the expense breakdown (legacy top-N shape).</summary>
public sealed record CategoryAmountBreakdownDto(Guid CategoryId, string CategoryName, decimal Amount);

/// <summary>One category slice for month reports / close-month JSON.</summary>
public sealed record MonthCategoryBreakdownItemDto(
    Guid? CategoryId,
    string CategoryName,
    decimal Amount,
    int TransactionCount,
    decimal PercentageOfTotalExpense);

/// <summary>Per-source expense totals for a month.</summary>
public sealed record MonthSourceBreakdownItemDto(Guid SourceId, string SourceName, decimal ExpenseAmount);

/// <summary>One day in a monthly cashflow grid.</summary>
public sealed record DailyCashflowDto(DateOnly Date, decimal Income, decimal Expense);

/// <summary>Large transaction row for report highlights.</summary>
public sealed record MonthlyReportTopTransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateOnly TxnDate,
    string? CategoryName,
    string SourceName);

/// <summary>Month-over-month % deltas (null when prior amount is 0).</summary>
public sealed record MonthOverMonthComparisonDto(
    decimal? IncomeChangePercent,
    decimal? ExpenseChangePercent,
    decimal? NetChangePercent);
