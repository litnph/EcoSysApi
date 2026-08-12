using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Computed + stored month snapshot for API responses.</summary>
public sealed record MonthlyPeriodSummaryDto(
    Guid? PeriodId,
    int Year,
    int Month,
    PeriodStatus Status,
    DateTime? ClosedAt,
    Guid? ClosedBy,
    long TotalIncome,
    long TotalExpense,
    long Net,
    IReadOnlyList<CategoryAmountBreakdownDto> TopExpenseCategories,
    IReadOnlyList<MonthCategoryBreakdownItemDto>? CategoryBreakdown = null,
    IReadOnlyList<MonthSourceBreakdownItemDto>? SourceBreakdown = null,
    string? Currency = null,
    bool ConsolidatedTotalsAvailable = true,
    IReadOnlyList<MonthlyCurrencySummaryDto>? CurrencyGroups = null);

/// <summary>Summary totals for exactly one currency.</summary>
public sealed record MonthlyCurrencySummaryDto(
    string Currency,
    long TotalIncome,
    long TotalExpense,
    long Net,
    decimal? SavingsRatePercent);
