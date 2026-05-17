using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed class GetDebtSummaryQueryHandler : IRequestHandler<GetDebtSummaryQuery, GetDebtSummaryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDebtSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetDebtSummaryResponse> Handle(GetDebtSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read debt summary for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeBorrowed = await _db.FinDebtRecords.AsNoTracking()
            .Where(r => r.SmoduleId == request.SmoduleId
                        && r.Status == DebtStatus.Active
                        && r.Direction == DebtDirection.Borrowed)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var activeLent = await _db.FinDebtRecords.AsNoTracking()
            .Where(r => r.SmoduleId == request.SmoduleId
                        && r.Status == DebtStatus.Active
                        && r.Direction == DebtDirection.Lent)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalBorrowedRemaining = activeBorrowed.Sum(r => r.RemainingAmount);
        var totalLentRemaining = activeLent.Sum(r => r.RemainingAmount);

        var overdueBorrowedCount = activeBorrowed.Count(r =>
            r.DueDate is { } d && d < today);

        var overdueLentCount = activeLent.Count(r =>
            r.DueDate is { } d && d < today);

        return new GetDebtSummaryResponse(totalBorrowedRemaining, totalLentRemaining, overdueBorrowedCount, overdueLentCount);
    }
}
