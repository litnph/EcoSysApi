using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.CreateMonthlyReport;

/// <summary>Creates an open monthly report for a calendar month.</summary>
public sealed record CreateMonthlyReportCommand(int Year, int Month) : IRequest<CreateMonthlyReportResponse>;
