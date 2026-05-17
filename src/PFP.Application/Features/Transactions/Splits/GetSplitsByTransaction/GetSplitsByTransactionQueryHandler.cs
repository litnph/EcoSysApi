using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Splits.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.GetSplitsByTransaction;

/// <summary>Loads splits after verifying read access to the parent transaction.</summary>
public sealed class GetSplitsByTransactionQueryHandler : IRequestHandler<GetSplitsByTransactionQuery, GetSplitsByTransactionResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSplitsByTransactionQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetSplitsByTransactionResponse> Handle(GetSplitsByTransactionQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var txn = await _db.FinTransactions
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
            throw new UnauthorizedAppException("You do not have permission to read this transaction.");

        if (_currentUser.CurrentOrgId is { } orgId && txn.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

        var rows = await _db.FinTxnSplits
            .AsNoTracking()
            .Where(s => s.TransactionId == request.TransactionId)
            .OrderBy(s => s.CreatedAt)
            .Select(s => new TxnSplitDto(
                s.Id,
                s.TransactionId,
                s.PersonName,
                s.PersonContact,
                s.Amount,
                s.Status,
                s.SettledAt,
                s.SettledTxnId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetSplitsByTransactionResponse(rows);
    }
}
