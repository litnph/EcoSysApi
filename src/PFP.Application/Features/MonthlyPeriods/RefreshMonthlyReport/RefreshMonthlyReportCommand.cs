using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;

/// <summary>Refreshes an open monthly report from live ledger data.</summary>
public sealed record RefreshMonthlyReportCommand(int Year, int Month) : IRequest<RefreshMonthlyReportResponse>;
