using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Rules for whether a transaction may be soft-deleted.</summary>
public static class TransactionDeletePolicy
{
    /// <summary>
    /// Delete is blocked for reversals, unsupported types, billing-cycle lines,
    /// installment-linked rows, completed transactions, and explicitly assigned
    /// closed monthly periods. A closed period matching only <c>TxnDate</c> does
    /// not by itself block deletion.
    /// </summary>
    public static async Task<bool> CanDeleteAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        CancellationToken cancellationToken = default)
    {
        if (txn.IsDeleted)
            return false;

        if (txn.Type == TransactionType.Reversal)
            return false;

        if (txn.Purpose is TransactionPurpose.SavingDeposit or TransactionPurpose.SavingWithdrawal
            || txn.Type == TransactionType.Transfer
               && txn.RefTxnId is null
               && txn.ExternalRef?.StartsWith("saving:", StringComparison.Ordinal) == true)
            return false;

        // Deletion deliberately does not use PostingPeriodPolicy here. That
        // policy also infers a closed month from TxnDate, while delete should
        // only respect the transaction's explicit/finalized state.
        if (txn.Status == TxnStatus.Completed)
            return false;

        if (txn.Type is not (
            TransactionType.Direct
            or TransactionType.Income
            or TransactionType.Transfer
            or TransactionType.Deferred))
            return false;

        if (txn.InstallmentPlanId is not null)
            return false;

        if (await db.FinInstallmentPlans.AsNoTracking()
                .AnyAsync(p => p.OriginalTxnId == txn.Id, cancellationToken)
                .ConfigureAwait(false))
            return false;

        // A transaction on an open statement may be deleted: the delete handler
        // removes the statement item and recalculates that cycle atomically.
        // Once the statement is locked, its historical lines remain immutable.
        if (await BillingCycleMembershipRules
                .HasActiveItemInLockedCycleAsync(db, txn.Id, cancellationToken)
                .ConfigureAwait(false))
            return false;

        if (txn.MonthlyPeriodId is { } mpId)
        {
            var period = await db.FinMonthlyPeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == mpId, cancellationToken)
                .ConfigureAwait(false);

            if (period is not null && period.Status != PeriodStatus.Open)
                return false;
        }

        return true;
    }

    /// <inheritdoc cref="CanDeleteAsync(IApplicationDbContext, FinTransaction, CancellationToken)"/>
    public static async Task<bool> CanDeleteAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var txn = await db.FinTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            .ConfigureAwait(false);

        return txn is not null && await CanDeleteAsync(db, txn, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc cref="CanDeleteAsync(IApplicationDbContext, Guid, CancellationToken)"/>
    public static async Task EnsureCanDeleteAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var txn = await db.FinTransactions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null)
            return;

        if (!await CanDeleteAsync(db, txn, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException(
                "Không thể xóa giao dịch thuộc kỳ sao kê đã khóa hoặc liên quan đến trả góp.");
    }
}
