using PFP.Application.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Copies computed report totals and breakdown JSON onto a monthly period row.</summary>
internal static class MonthlyReportPeriodWriter
{
    public static void Apply(FinMonthlyPeriod period, MonthlyReportDto report, DateTime refreshedAtUtc)
    {
        period.TotalIncome = CurrencyUnits.FromWhole(report.Summary.TotalIncome);
        period.TotalExpense = CurrencyUnits.FromWhole(report.Summary.TotalExpense);
        period.Net = CurrencyUnits.FromWhole(report.Summary.Net);
        period.CategoryBreakdown = MonthlyPeriodSummaryCalculator.SerialiseCategoryBreakdown(report.CategoryBreakdown);
        period.SourceBreakdown = MonthlyPeriodSummaryCalculator.SerialiseSourceBreakdown(report.SourceBreakdown);
        period.ReportSnapshot = MonthlyReportSnapshotStore.Serialize(report);
        period.LastRefreshedAt = refreshedAtUtc;
    }
}
