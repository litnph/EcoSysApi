using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.DeleteMonthlyReport;

/// <summary>Deletes a monthly report row and reverts month-close locks on transactions.</summary>
public sealed class DeleteMonthlyReportCommandHandler
    : IRequestHandler<DeleteMonthlyReportCommand, DeleteMonthlyReportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteMonthlyReportCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DeleteMonthlyReportResponse> Handle(
        DeleteMonthlyReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var period = await _db.FinMonthlyPeriods
            .FirstOrDefaultAsync(
                p => p.Year == request.Year && p.Month == request.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (period is null || period.ReportCreatedAt is null)
            throw new NotFoundException("Monthly report was not found.");

        if (period.Status == PeriodStatus.Closed)
        {
            var linkedTxns = await _db.FinTransactions
                .Where(t => t.MonthlyPeriodId == period.Id && !t.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var txn in linkedTxns)
            {
                txn.MonthlyPeriodId = null;
                if (txn.Status == TxnStatus.Completed)
                    txn.Status = TxnStatus.New;
            }
        }

        _db.FinMonthlyPeriods.Remove(period);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteMonthlyReportResponse(request.Year, request.Month);
    }
}
