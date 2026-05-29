using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.CreateMonthlyReport;

/// <summary>Created report payload.</summary>
public sealed record CreateMonthlyReportResponse(MonthlyReportDto Report);
