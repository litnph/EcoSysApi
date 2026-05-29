namespace PFP.Application.Features.MonthlyPeriods.DeleteMonthlyReport;

/// <summary>Result of <see cref="DeleteMonthlyReportCommand"/>.</summary>
public sealed record DeleteMonthlyReportResponse(int Year, int Month);
