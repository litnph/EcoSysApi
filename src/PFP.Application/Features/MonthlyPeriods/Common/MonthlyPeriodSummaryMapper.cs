using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

internal static class MonthlyPeriodSummaryMapper
{
    public static MonthlyPeriodSummaryDto FromReport(
        MonthlyReportDto report,
        Guid? periodId,
        int year,
        int month,
        PeriodStatus status,
        DateTime? closedAt,
        Guid? closedBy)
    {
        var top = report.CategoryBreakdown
            .Where(category => category.CategoryId.HasValue)
            .Take(5)
            .Select(category => new CategoryAmountBreakdownDto(
                category.CategoryId!.Value,
                category.CategoryName,
                category.Amount))
            .ToList();

        var groups = (report.CurrencyGroups ?? [])
            .Select(group => new MonthlyCurrencySummaryDto(
                group.Currency,
                group.Summary.TotalIncome,
                group.Summary.TotalExpense,
                group.Summary.Net,
                group.Summary.SavingsRatePercent))
            .ToList();

        return new MonthlyPeriodSummaryDto(
            periodId,
            year,
            month,
            status,
            closedAt,
            closedBy,
            report.Summary.TotalIncome,
            report.Summary.TotalExpense,
            report.Summary.Net,
            top,
            report.CategoryBreakdown,
            report.SourceBreakdown,
            report.Metadata?.Currency,
            report.Metadata?.ConsolidatedTotalsAvailable ?? true,
            groups);
    }
}
