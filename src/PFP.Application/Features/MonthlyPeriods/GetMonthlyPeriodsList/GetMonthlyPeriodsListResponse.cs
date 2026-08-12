using PFP.Domain.Enums;
using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>One user-created monthly report row.</summary>
public sealed record MonthlyPeriodListItemDto(
    int Year,
    int Month,
    PeriodStatus Status,
    long TotalIncome,
    long TotalExpense,
    long Net,
    DateTime ReportCreatedAt,
    DateTime? LastRefreshedAt,
    DateTime? ClosedAt,
    string? Currency,
    bool ConsolidatedTotalsAvailable,
    IReadOnlyList<MonthlyCurrencySummaryDto>? CurrencyGroups = null);

/// <summary>Created monthly reports (newest first).</summary>
public sealed record GetMonthlyPeriodsListResponse(IReadOnlyList<MonthlyPeriodListItemDto> Periods);
