using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var utc = DateTime.UtcNow;
        var year = utc.Year;
        var month = utc.Month;

        var (income, expense, top) = await MonthlyPeriodSummaryCalculator
            .ComputeAsync(_db, request.SmoduleId, year, month, cancellationToken)
            .ConfigureAwait(false);

        var period = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.SmoduleId == request.SmoduleId && p.Year == year && p.Month == month,
                cancellationToken)
            .ConfigureAwait(false);

        var net = income - expense;
        var dto = new MonthlyPeriodSummaryDto(
            period?.Id,
            request.SmoduleId,
            year,
            month,
            period?.Status ?? PeriodStatus.Open,
            period?.ClosedAt,
            period?.ClosedBy,
            income,
            expense,
            net,
            top);

        return new GetCurrentMonthSummaryResponse(dto);
    }
}
