using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Rules for whether a transaction amount may be edited in place.</summary>
public static class TransactionAmountEditPolicy
{
    /// <summary>
    /// Amount is locked when the transaction was converted to an installment plan
    /// or is still listed on a billing cycle.
    /// </summary>
    public static async Task<bool> CanEditAmountAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        if (await db.FinInstallmentPlans.AsNoTracking()
                .AnyAsync(p => p.OriginalTxnId == transactionId, cancellationToken)
                .ConfigureAwait(false))
            return false;

        if (await db.FinBillingCycleItems.AsNoTracking()
                .AnyAsync(
                    i => i.TransactionId == transactionId && i.RemovedAt == null,
                    cancellationToken)
                .ConfigureAwait(false))
            return false;

        return true;
    }

    /// <inheritdoc cref="CanEditAmountAsync"/>
    public static async Task EnsureCanEditAmountAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        if (!await CanEditAmountAsync(db, transactionId, cancellationToken).ConfigureAwait(false))
            throw new BusinessRuleException(
                "Cannot edit the amount of a transaction that is on a billing cycle or was converted to an installment plan.");
    }
}
