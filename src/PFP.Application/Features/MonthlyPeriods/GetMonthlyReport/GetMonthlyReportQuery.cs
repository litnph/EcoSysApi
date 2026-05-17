using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Full month analytics for a finance module.</summary>
public sealed record GetMonthlyReportQuery(Guid SmoduleId, int Year, int Month) : IRequest<GetMonthlyReportResponse>;
