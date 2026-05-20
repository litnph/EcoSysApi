using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriod;

/// <summary>Reads a calendar month summary for a finance module.</summary>
public sealed record GetMonthlyPeriodQuery(int Year, int Month) : IRequest<GetMonthlyPeriodResponse>;
