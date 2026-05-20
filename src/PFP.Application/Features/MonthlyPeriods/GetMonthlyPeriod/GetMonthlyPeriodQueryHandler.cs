using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriod;

/// <summary>Builds the month summary for an arbitrary calendar month.</summary>
public sealed class GetMonthlyPeriodQueryHandler : IRequestHandler<GetMonthlyPeriodQuery, GetMonthlyPeriodResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetMonthlyPeriodQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{GetMonthlyPeriodQuery, GetMonthlyPeriodResponse}.Handle" />
    public async Task<GetMonthlyPeriodResponse> Handle(GetMonthlyPeriodQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var (income, expense, top) = await MonthlyPeriodSummaryCalculator
            .ComputeAsync(_db, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        var period = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Year == request.Year && p.Month == request.Month,
                cancellationToken)
            .ConfigureAwait(false);

        var net = income - expense;
        var dto = new MonthlyPeriodSummaryDto(
            period?.Id,
            request.Year,
            request.Month,
            period?.Status ?? PeriodStatus.Open,
            period?.ClosedAt,
            period?.ClosedBy,
            CurrencyUnits.ToWhole(income),
            CurrencyUnits.ToWhole(expense),
            CurrencyUnits.ToWhole(net),
            top);

        return new GetMonthlyPeriodResponse(dto);
    }
}
