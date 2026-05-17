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

        var report = await MonthlyPeriodSummaryCalculator
            .BuildReportAsync(_db, request.SmoduleId, request.Year, request.Month, cancellationToken)
            .ConfigureAwait(false);

        return new GetMonthlyReportResponse(report);
    }
}
