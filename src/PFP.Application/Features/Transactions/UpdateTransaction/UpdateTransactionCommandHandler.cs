using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.UpdateTransaction;

/// <summary>
/// Updates the editable metadata of an existing <c>FIN_TRANSACTION</c>. Balance-affecting fields
/// (Amount / Type / SourceId / DestSourceId) are not exposed by this command — modifying those
/// requires a soft-delete + reversal cycle per spec §4.2 and §4.6.
/// </summary>
public sealed class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, UpdateTransactionResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateTransactionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{UpdateTransactionCommand, UpdateTransactionResponse}.Handle" />
    public async Task<UpdateTransactionResponse> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var txn = await _db.FinTransactions
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null || txn.IsDeleted)
            throw new NotFoundException("Transaction was not found.");

        if (txn.Type == TransactionType.Reversal)
            throw new BusinessRuleException("Reversal transactions are immutable.");

        if (request.CategoryId is { } categoryId)
        {
            var categoryExists = await _db.FinCategories
                .AnyAsync(c => c.Id == categoryId, cancellationToken)
                .ConfigureAwait(false);
            if (!categoryExists)
                throw new NotFoundException("Category was not found in this module.");
        }

        if (request.MonthlyPeriodId is { } mpId)
        {
            var mpExists = await _db.FinMonthlyPeriods
                .AnyAsync(p => p.Id == mpId, cancellationToken)
                .ConfigureAwait(false);
            if (!mpExists)
                throw new NotFoundException("Monthly period was not found in this module.");
        }

        txn.CategoryId = request.CategoryId;
        txn.TxnDate = request.TxnDate;
        txn.Description = request.Description.Trim();
        txn.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        txn.MonthlyPeriodId = request.MonthlyPeriodId;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var refreshed = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateTransactionResponse(TransactionDtoMapper.ToDetail(refreshed));
    }
}
