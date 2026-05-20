using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;

/// <summary>Builds the live month summary for the finance module.</summary>
public sealed class GetCurrentMonthSummaryQueryHandler : IRequestHandler<GetCurrentMonthSummaryQuery, GetCurrentMonthSummaryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetCurrentMonthSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{GetCurrentMonthSummaryQuery, GetCurrentMonthSummaryResponse}.Handle" />
    public async Task<GetCurrentMonthSummaryResponse> Handle(GetCurrentMonthSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var utc = DateTime.UtcNow;
        var year = utc.Year;
        var month = utc.Month;

        var (income, expense, top) = await MonthlyPeriodSummaryCalculator
            .ComputeAsync(_db, year, month, cancellationToken)
            .ConfigureAwait(false);

        var period = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Year == year && p.Month == month,
                cancellationToken)
            .ConfigureAwait(false);

        var net = income - expense;
        var dto = new MonthlyPeriodSummaryDto(
            period?.Id,
            year,
            month,
            period?.Status ?? PeriodStatus.Open,
            period?.ClosedAt,
            period?.ClosedBy,
            CurrencyUnits.ToWhole(income),
            CurrencyUnits.ToWhole(expense),
            CurrencyUnits.ToWhole(net),
            top);

        return new GetCurrentMonthSummaryResponse(dto);
    }
}
