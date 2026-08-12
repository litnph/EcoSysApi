using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
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

        OptimisticConcurrencyGuard.Ensure(txn.Version, request.ExpectedVersion);

        if (txn.Type == TransactionType.Reversal)
            throw new BusinessRuleException("Reversal transactions are immutable.");

        await PostingPeriodPolicy
            .EnsureExistingTransactionMutableAsync(_db, txn, cancellationToken)
            .ConfigureAwait(false);

        await PostingPeriodPolicy
            .EnsureOpenTargetAsync(_db, request.TxnDate, request.MonthlyPeriodId, cancellationToken)
            .ConfigureAwait(false);

        FinCategory? category = null;
        if (request.CategoryId is { } categoryId)
        {
            category = await _db.FinCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
                .ConfigureAwait(false);
            if (category is null)
                throw new NotFoundException("Category was not found in this module.");

            var expectedKind = txn.Type == TransactionType.Income ? CategoryKind.Income : CategoryKind.Expense;
            if (txn.Type is TransactionType.Direct or TransactionType.Deferred or TransactionType.Split or TransactionType.Income
                && category.Kind != expectedKind)
                throw new BusinessRuleException("Category kind is incompatible with the transaction type.");
        }

        if (request.MonthlyPeriodId is { } mpId)
        {
            var mpExists = await _db.FinMonthlyPeriods
                .AnyAsync(p => p.Id == mpId, cancellationToken)
                .ConfigureAwait(false);
            if (!mpExists)
                throw new NotFoundException("Monthly period was not found in this module.");
        }

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            if (request.Amount is { } whole)
            {
                await TransactionAmountEditPolicy
                    .EnsureCanEditAmountAsync(_db, txn.Id, ct)
                    .ConfigureAwait(false);

                var newAmount = txn.Type == TransactionType.BalanceAdjustment
                    ? CurrencyUnits.FromWhole(whole)
                    : CurrencyUnits.FromWhole(Math.Abs(whole));

                await TransactionAmountUpdater
                    .ApplyAsync(_db, txn, newAmount, ct)
                    .ConfigureAwait(false);
            }

            txn.CategoryId = request.CategoryId;
            txn.TxnDate = request.TxnDate;
            txn.Description = string.IsNullOrWhiteSpace(request.Description)
                ? string.Empty
                : request.Description.Trim();
            txn.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            txn.MonthlyPeriodId = request.MonthlyPeriodId;

            if (txn.Type == TransactionType.Transfer && txn.RefTxnId is { } partnerId)
            {
                var partner = await _db.FinTransactions
                    .FirstOrDefaultAsync(t => t.Id == partnerId, ct)
                    .ConfigureAwait(false);

                if (partner is null || partner.RefTxnId != txn.Id || partner.IsDeleted)
                    throw new BusinessRuleException("The linked transfer counterpart is missing or inconsistent.");

                await PostingPeriodPolicy
                    .EnsureExistingTransactionMutableAsync(_db, partner, ct)
                    .ConfigureAwait(false);

                partner.TxnDate = request.TxnDate;
                partner.Description = txn.Description;
                partner.Note = txn.Note;
                partner.MonthlyPeriodId = request.MonthlyPeriodId;
            }

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        var refreshed = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        var canEditAmount = await TransactionAmountEditPolicy
            .CanEditAmountAsync(_db, refreshed.Id, cancellationToken)
            .ConfigureAwait(false);

        var canDelete = await TransactionDeletePolicy
            .CanDeleteAsync(_db, refreshed, cancellationToken)
            .ConfigureAwait(false);

        var hasInstallmentPlan = await _db.FinInstallmentPlans
            .AsNoTracking()
            .AnyAsync(p => p.OriginalTxnId == refreshed.Id, cancellationToken)
            .ConfigureAwait(false);

        return new UpdateTransactionResponse(
            TransactionDtoMapper.ToDetail(
                refreshed,
                canEditAmount,
                canDelete,
                hasInstallmentPlan,
                refreshed.InstallmentPlanId is not null));
    }
}
