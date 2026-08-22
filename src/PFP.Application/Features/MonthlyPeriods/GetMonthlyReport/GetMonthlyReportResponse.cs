using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Wrapped report for API.</summary>
public sealed record GetMonthlyReportResponse(
    MonthlyReportDto Report,
    PeriodStatus Status);
