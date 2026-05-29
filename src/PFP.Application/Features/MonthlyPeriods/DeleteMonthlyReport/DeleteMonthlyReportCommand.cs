using MediatR;

namespace PFP.Application.Features.MonthlyPeriods.DeleteMonthlyReport;

/// <summary>Removes a user-created monthly report and unlocks linked transactions when closed.</summary>
public sealed record DeleteMonthlyReportCommand(int Year, int Month) : IRequest<DeleteMonthlyReportResponse>;
