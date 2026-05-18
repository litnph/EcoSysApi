using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecordDetail;

public sealed class GetDebtRecordDetailQueryHandler : IRequestHandler<GetDebtRecordDetailQuery, GetDebtRecordDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDebtRecordDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetDebtRecordDetailResponse> Handle(GetDebtRecordDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var record = await _db.FinDebtRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.DebtRecordId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null)
            throw new NotFoundException("Debt record was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(record.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this debt record.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == record.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this debt record.");

        var txRows = await _db.FinDebtTransactions
            .AsNoTracking()
            .Where(t => t.DebtRecordId == record.Id)
            .OrderBy(t => t.TxnDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var txns = txRows
            .Select(t => new DebtTransactionItemDto(t.Id, t.TxnId, CurrencyUnits.ToWhole(t.Amount), t.Type, t.Note, t.TxnDate, t.CreatedAt))
            .ToList();

        var dto = new DebtRecordDetailDto(
            record.Id,
            record.SmoduleId,
            record.Direction,
            record.PersonName,
            record.PersonContact,
            record.OriginalTxnId,
            CurrencyUnits.ToWhole(record.OriginalAmount),
            CurrencyUnits.ToWhole(record.RemainingAmount),
            record.Currency,
            record.DueDate,
            record.Status,
            record.Note,
            record.Version,
            record.CreatedAt,
            record.UpdatedAt,
            txns);

        return new GetDebtRecordDetailResponse(dto);
    }
}
