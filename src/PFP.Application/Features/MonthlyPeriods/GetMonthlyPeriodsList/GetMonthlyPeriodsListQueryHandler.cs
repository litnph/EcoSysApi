using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>Returns monthly reports explicitly created by the user.</summary>
public sealed class GetMonthlyPeriodsListQueryHandler : IRequestHandler<GetMonthlyPeriodsListQuery, GetMonthlyPeriodsListResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlyPeriodsListQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetMonthlyPeriodsListResponse> Handle(GetMonthlyPeriodsListQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var rows = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .Where(p => p.ReportCreatedAt != null)
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var list = rows.Select(row => new MonthlyPeriodListItemDto(
            row.Year,
            row.Month,
            row.Status,
            CurrencyUnits.ToWhole(row.TotalIncome),
            CurrencyUnits.ToWhole(row.TotalExpense),
            CurrencyUnits.ToWhole(row.Net),
            row.ReportCreatedAt!.Value,
            row.LastRefreshedAt,
            row.ClosedAt)).ToList();

        return new GetMonthlyPeriodsListResponse(list);
    }
}
