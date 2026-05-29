using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.CreateMonthlyReport;

/// <summary>Creates a monthly report row and stores the first snapshot.</summary>
public sealed class CreateMonthlyReportCommandHandler
    : IRequestHandler<CreateMonthlyReportCommand, CreateMonthlyReportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateMonthlyReportCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateMonthlyReportResponse> Handle(
        CreateMonthlyReportCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var existing = await _db.FinMonthlyPeriods
            .FirstOrDefaultAsync(p => p.Year == request.Year && p.Month == request.Month, cancellationToken)
            .ConfigureAwait(false);

        if (existing?.ReportCreatedAt is not null)
            throw new BusinessRuleException("Monthly report for this month already exists.");

        if (existing?.Status == PeriodStatus.Closed)
            throw new BusinessRuleException("This month is already closed.");

        var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(_db, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        var utcNow = DateTime.UtcNow;
        var period = existing ?? new FinMonthlyPeriod
        {
            Year = request.Year,
            Month = request.Month,
        };

        if (existing is null)
            _db.FinMonthlyPeriods.Add(period);

        period.ReportCreatedAt = utcNow;
        period.Status = PeriodStatus.Open;
        MonthlyReportPeriodWriter.Apply(period, report, utcNow);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new CreateMonthlyReportResponse(report);
    }
}
