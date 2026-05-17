using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Computed + stored month snapshot for API responses.</summary>
public sealed record MonthlyPeriodSummaryDto(
    Guid? PeriodId,
    Guid SmoduleId,
    int Year,
    int Month,
    PeriodStatus Status,
    DateTime? ClosedAt,
    Guid? ClosedBy,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Net,
    IReadOnlyList<CategoryAmountBreakdownDto> TopExpenseCategories,
    IReadOnlyList<MonthCategoryBreakdownItemDto>? CategoryBreakdown = null,
    IReadOnlyList<MonthSourceBreakdownItemDto>? SourceBreakdown = null);
