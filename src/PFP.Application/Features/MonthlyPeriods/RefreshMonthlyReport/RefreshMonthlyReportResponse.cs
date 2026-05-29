using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;

/// <summary>Refreshed report payload.</summary>
public sealed record RefreshMonthlyReportResponse(MonthlyReportDto Report);
