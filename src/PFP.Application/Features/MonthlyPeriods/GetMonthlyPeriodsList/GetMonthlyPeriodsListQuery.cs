using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>Lists the last 12 calendar months for a finance module.</summary>
public sealed record GetMonthlyPeriodsListQuery() : IRequest<GetMonthlyPeriodsListResponse>;
