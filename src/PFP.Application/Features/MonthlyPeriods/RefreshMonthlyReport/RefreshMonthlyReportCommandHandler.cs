using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;

/// <summary>Recomputes and stores a snapshot for an open monthly report.</summary>
public sealed class RefreshMonthlyReportCommandHandler
    : IRequestHandler<RefreshMonthlyReportCommand, RefreshMonthlyReportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RefreshMonthlyReportCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefreshMonthlyReportResponse> Handle(
        RefreshMonthlyReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var period = await _db.FinMonthlyPeriods
            .FirstOrDefaultAsync(p => p.Year == request.Year && p.Month == request.Month, cancellationToken)
            .ConfigureAwait(false);

        if (period is null || period.ReportCreatedAt is null)
            throw new NotFoundException("Monthly report was not found.");

        if (period.Status == PeriodStatus.Closed)
            throw new BusinessRuleException("Closed monthly reports cannot be refreshed.");

        var userId = _currentUser.UserId.Value;
        var reportDay = await MonthlyReportUserPreferences
            .GetReportDayAsync(_db, userId, period.ReportSnapshot, cancellationToken)
            .ConfigureAwait(false);
        var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(
                _db,
                request.Year,
                request.Month,
                userId,
                reportDay,
                cancellationToken,
                new MonthlyReportTransactionOwnership.ReportIdentity(
                    period.Id,
                    period.ReportCreatedAt.Value))
            .ConfigureAwait(false);

        await MonthlyReportTransactionOwnership
            .SynchronizeAsync(_db, period.Id, report, cancellationToken)
            .ConfigureAwait(false);
        MonthlyReportPeriodWriter.Apply(period, report, DateTime.UtcNow);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new RefreshMonthlyReportResponse(report);
    }
}
