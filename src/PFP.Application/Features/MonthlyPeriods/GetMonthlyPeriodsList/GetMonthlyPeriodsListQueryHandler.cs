using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>Rolls back 12 months from UTC "today" and joins stored <c>fin_monthly_periods</c> rows.</summary>
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

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this finance module.");

        var smodule = await _db.SpaceModules
            .AsNoTracking()
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var anchorYear = today.Year;
        var anchorMonth = today.Month;

        var keys = new List<(int Y, int M)>(12);
        var y = anchorYear;
        var m = anchorMonth;
        for (var i = 0; i < 12; i++)
        {
            keys.Add((y, m));
            if (m == 1)
            {
                y--;
                m = 12;
            }
            else
            {
                m--;
            }
        }

        var periods = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .Where(p => p.SmoduleId == request.SmoduleId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dict = periods.ToDictionary(p => (p.Year, p.Month));

        var list = keys.Select(key =>
        {
            if (dict.TryGetValue(key, out var row))
            {
                return new MonthlyPeriodListItemDto(
                    row.Year,
                    row.Month,
                    row.Status,
                    CurrencyUnits.ToWhole(row.TotalIncome),
                    CurrencyUnits.ToWhole(row.TotalExpense),
                    CurrencyUnits.ToWhole(row.Net));
            }

            return new MonthlyPeriodListItemDto(key.Y, key.M, PeriodStatus.Open, 0, 0, 0);
        }).ToList();

        return new GetMonthlyPeriodsListResponse(list);
    }
}
