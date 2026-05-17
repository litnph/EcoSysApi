using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Wrapped report for API.</summary>
public sealed record GetMonthlyReportResponse(MonthlyReportDto Report);
