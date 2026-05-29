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
    /// installment-linked rows, and closed monthly periods.
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

        if (await BillingCycleMembershipRules
                .HasActiveItemOnAnyCycleAsync(db, txn.Id, cancellationToken)
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

    /// <inheritdoc cref="CanDeleteAsync"/>
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

    /// <inheritdoc cref="CanDeleteAsync"/>
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
                "Không thể xóa giao dịch đã nằm trong kỳ sao kê hoặc liên quan đến trả góp.");
    }
}
