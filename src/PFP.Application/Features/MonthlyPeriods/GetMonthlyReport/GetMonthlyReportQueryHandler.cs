using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Builds <see cref="MonthlyReportDto"/> from ledger rows.</summary>
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
var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(_db, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        return new GetMonthlyReportResponse(report);
    }
}
