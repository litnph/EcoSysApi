using MediatR;
using PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;

namespace PFP.Application.Features.MonthlyPeriods.AddMonthlyReportTransaction;

public sealed record AddMonthlyReportTransactionCommand(
    int Year,
    int Month,
    Guid TransactionId) : IRequest<RefreshMonthlyReportResponse>;
