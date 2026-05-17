using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;

/// <summary>Envelope for <see cref="GetCurrentMonthSummaryQuery"/>.</summary>
public sealed record GetCurrentMonthSummaryResponse(MonthlyPeriodSummaryDto Summary);
