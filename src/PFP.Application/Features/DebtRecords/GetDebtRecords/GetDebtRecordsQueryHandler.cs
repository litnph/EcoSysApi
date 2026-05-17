using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed class GetDebtRecordsQueryHandler : IRequestHandler<GetDebtRecordsQuery, GetDebtRecordsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDebtRecordsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetDebtRecordsResponse> Handle(GetDebtRecordsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read debt records for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.FinDebtRecords.AsNoTracking().Where(r => r.SmoduleId == request.SmoduleId);

        if (request.Direction is { } dir)
            query = query.Where(r => r.Direction == dir);

        if (request.Status is { } st)
            query = query.Where(r => r.Status == st);

        var rows = await query
            .OrderBy(r => r.Status != DebtStatus.Active ? 1 : 0)
            .ThenBy(r => r.DueDate ?? DateOnly.MaxValue)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows.Select(r => new DebtRecordListItemDto(
            r.Id,
            r.SmoduleId,
            r.Direction,
            r.PersonName,
            r.PersonContact,
            r.OriginalAmount,
            r.RemainingAmount,
            r.Currency,
            r.DueDate,
            r.Status,
            r.DueDate is { } due ? (int?)(due.DayNumber - today.DayNumber) : null,
            r.CreatedAt)).ToList();

        return new GetDebtRecordsResponse(items);
    }
}
