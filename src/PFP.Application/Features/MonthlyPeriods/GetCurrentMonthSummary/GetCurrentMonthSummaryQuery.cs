using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;

/// <summary>Reads the UTC current calendar month summary for a finance module.</summary>
public sealed record GetCurrentMonthSummaryQuery() : IRequest<GetCurrentMonthSummaryResponse>;
