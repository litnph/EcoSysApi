using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriod;

/// <summary>Envelope for <see cref="GetMonthlyPeriodQuery"/>.</summary>
public sealed record GetMonthlyPeriodResponse(MonthlyPeriodSummaryDto Summary);
