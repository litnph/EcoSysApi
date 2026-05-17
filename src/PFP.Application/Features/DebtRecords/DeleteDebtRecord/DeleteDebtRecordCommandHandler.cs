using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.DeleteDebtRecord;

public sealed class DeleteDebtRecordCommandHandler : IRequestHandler<DeleteDebtRecordCommand, DeleteDebtRecordResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteDebtRecordCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<DeleteDebtRecordResponse> Handle(DeleteDebtRecordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var record = await _db.FinDebtRecords
            .Include(r => r.FinDebtTransactions)
            .Include(r => r.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(r => r.Id == request.DebtRecordId, cancellationToken)
            .ConfigureAwait(false);

        if (record is null || record.IsDeleted)
            throw new NotFoundException("Debt record was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(record.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to delete this debt record.");

        if (_currentUser.CurrentOrgId is { } orgId && record.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this debt record.");

        if (record.FinDebtTransactions.Count > 0)
            throw new BusinessRuleException("Cannot delete a debt record that already has repayment or collection movements.");

        _db.FinDebtRecords.Remove(record);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteDebtRecordResponse(record.Id);
    }
}
