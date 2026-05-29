using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Builds or loads <see cref="MonthlyReportDto"/> for a created monthly report.</summary>
public sealed class GetMonthlyReportQueryHandler : IRequestHandler<GetMonthlyReportQuery, GetMonthlyReportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlyReportQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetMonthlyReportResponse> Handle(GetMonthlyReportQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var period = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Year == request.Year && p.Month == request.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (period is null || period.ReportCreatedAt is null)
            throw new NotFoundException("Monthly report was not found.");

        MonthlyReportDto report;
        if (period.Status == PeriodStatus.Closed && !string.IsNullOrWhiteSpace(period.ReportSnapshot))
        {
            report = MonthlyReportSnapshotStore.Deserialize(period.ReportSnapshot);
        }
        else
        {
            report = await MonthlyPeriodSummaryCalculator
                .BuildReportAsync(_db, request.Year, request.Month, cancellationToken)
                .ConfigureAwait(false);
        }

        return new GetMonthlyReportResponse(report);
    }
}
