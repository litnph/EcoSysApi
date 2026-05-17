using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactionHistory;

/// <summary>Returns fin_transaction_history rows for a transaction, oldest version first.</summary>
public sealed class GetTransactionHistoryQueryHandler : IRequestHandler<GetTransactionHistoryQuery, GetTransactionHistoryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetTransactionHistoryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{GetTransactionHistoryQuery, GetTransactionHistoryResponse}.Handle" />
    public async Task<GetTransactionHistoryResponse> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var txn = await _db.FinTransactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(t => t.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null)
            throw new NotFoundException("Transaction was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(txn.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this transaction history.");

        if (_currentUser.CurrentOrgId is { } orgId && txn.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        var rows = await _db.FinTransactionHistory
            .AsNoTracking()
            .Where(h => h.TransactionId == request.TransactionId)
            .OrderBy(h => h.Version)
            .ThenBy(h => h.CreatedAt)
            .Select(h => new TransactionHistoryItemDto(
                h.Id,
                h.TransactionId,
                h.Version,
                h.ChangedBy,
                h.SessionId,
                h.ChangeType,
                h.ChangedFields,
                h.Snapshot,
                h.ChangeReason,
                h.CreatedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetTransactionHistoryResponse(rows);
    }
}
