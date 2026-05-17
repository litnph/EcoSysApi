using PFP.Application.Features.MonthlyPeriods.Common;

namespace PFP.Application.Features.MonthlyPeriods.CloseMonth;

/// <summary>Payload returned after <see cref="CloseMonthCommand"/>.</summary>
public sealed record CloseMonthResponse(MonthlyPeriodSummaryDto Summary);
