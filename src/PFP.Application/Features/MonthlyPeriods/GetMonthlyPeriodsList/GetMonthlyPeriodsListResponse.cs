using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>One row per calendar month (most recent first).</summary>
public sealed record MonthlyPeriodListItemDto(
    int Year,
    int Month,
    PeriodStatus Status,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Net);

/// <summary>Twelve month slots.</summary>
public sealed record GetMonthlyPeriodsListResponse(IReadOnlyList<MonthlyPeriodListItemDto> Periods);
