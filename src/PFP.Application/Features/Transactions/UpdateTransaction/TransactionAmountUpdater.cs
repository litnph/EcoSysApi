using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.UpdateTransaction;

/// <summary>Updates transaction amount and reconciles balances and linked records.</summary>
internal static class TransactionAmountUpdater
{
    public static async Task ApplyAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAmount,
        CancellationToken cancellationToken)
    {
        if (txn.Type == TransactionType.Reversal)
            throw new BusinessRuleException("Reversal transactions are immutable.");

        if (txn.InstallmentPlanId is not null)
            throw new BusinessRuleException("Installment payment amounts cannot be edited here.");

        await TransactionAmountEditPolicy
            .EnsureCanEditAmountAsync(db, txn.Id, cancellationToken)
            .ConfigureAwait(false);

        var oldAmount = txn.Amount;
        if (oldAmount == newAmount)
            return;

        switch (txn.Type)
        {
            case TransactionType.Transfer:
                await ApplyTransferAsync(db, txn, newAmount, cancellationToken).ConfigureAwait(false);
                return;
            case TransactionType.Split:
                await ApplySplitAsync(db, txn, newAmount, oldAmount, cancellationToken)
                    .ConfigureAwait(false);
                break;
            case TransactionType.DebtRepay:
            case TransactionType.LoanCollect:
                await ApplyDebtPaymentAsync(db, txn, newAmount, oldAmount, cancellationToken)
                    .ConfigureAwait(false);
                break;
            case TransactionType.DebtBorrow:
            case TransactionType.LoanGive:
                await ApplyDebtOriginationAsync(db, txn, newAmount, oldAmount, cancellationToken)
                    .ConfigureAwait(false);
                break;
            default:
                await ApplySimpleAsync(db, txn, newAmount, oldAmount, cancellationToken)
                    .ConfigureAwait(false);
                break;
        }

        txn.Amount = newAmount;

        if (txn.Type == TransactionType.Deferred)
            await RecalculateDeferredBillingCycleAsync(db, txn.Id, cancellationToken)
                .ConfigureAwait(false);
    }

    private static async Task ApplySimpleAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAmount,
        decimal oldAmount,
        CancellationToken cancellationToken)
    {
        var source = await RequireSourceAsync(db, txn.SourceId, cancellationToken).ConfigureAwait(false);
        EnsureSufficientBalance(source, txn.Type, oldAmount, newAmount);
        source.Balance += TransactionBalanceLegMath.BalanceChange(txn.Type, oldAmount, newAmount);
    }

    private static async Task ApplyTransferAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAbsAmount,
        CancellationToken cancellationToken)
    {
        if (txn.RefTxnId is null)
            throw new BusinessRuleException("Transfer transaction is missing a counterpart link.");

        var partner = await db.FinTransactions
            .Include(t => t.Source)
            .FirstOrDefaultAsync(t => t.Id == txn.RefTxnId.Value, cancellationToken)
            .ConfigureAwait(false);

        if (partner is null || partner.IsDeleted || partner.Type != TransactionType.Transfer)
            throw new BusinessRuleException("The linked transfer counterpart is missing or invalid.");

        var outbound = txn.Amount < 0 ? txn : partner;
        var inbound = txn.Amount < 0 ? partner : txn;
        var signedNewOut = -Math.Abs(newAbsAmount);
        var signedNewIn = Math.Abs(newAbsAmount);

        var fromSource = outbound.Source
            ?? await RequireSourceAsync(db, outbound.SourceId, cancellationToken).ConfigureAwait(false);
        var toSource = inbound.Source
            ?? await RequireSourceAsync(db, inbound.SourceId, cancellationToken).ConfigureAwait(false);

        var outChange = TransactionBalanceLegMath.BalanceChange(
            TransactionType.Transfer,
            outbound.Amount,
            signedNewOut);
        if (outChange < 0 && fromSource.Type != SourceType.CreditCard && fromSource.Balance + outChange < 0)
            throw new BusinessRuleException("Insufficient balance on the outbound source.");

        fromSource.Balance += outChange;
        toSource.Balance += TransactionBalanceLegMath.BalanceChange(
            TransactionType.Transfer,
            inbound.Amount,
            signedNewIn);

        outbound.Amount = signedNewOut;
        inbound.Amount = signedNewIn;
    }

    private static async Task ApplySplitAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAmount,
        decimal oldAmount,
        CancellationToken cancellationToken)
    {
        var hasSettled = await db.FinTxnSplits
            .AnyAsync(
                s => s.TransactionId == txn.Id && !s.IsDeleted && s.Status == SplitStatus.Settled,
                cancellationToken)
            .ConfigureAwait(false);

        if (hasSettled)
            throw new BusinessRuleException("Cannot change amount after a split line has been settled.");

        var source = await RequireSourceAsync(db, txn.SourceId, cancellationToken).ConfigureAwait(false);
        EnsureSufficientBalance(source, txn.Type, oldAmount, newAmount);
        source.Balance += TransactionBalanceLegMath.BalanceChange(txn.Type, oldAmount, newAmount);
    }

    private static async Task ApplyDebtPaymentAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAmount,
        decimal oldAmount,
        CancellationToken cancellationToken)
    {
        var leg = await db.FinDebtTransactions
            .FirstOrDefaultAsync(d => d.TxnId == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        if (leg is null)
            throw new BusinessRuleException("Linked debt transaction row was not found.");

        var debt = await db.FinDebtRecords
            .FirstOrDefaultAsync(d => d.Id == leg.DebtRecordId, cancellationToken)
            .ConfigureAwait(false);

        if (debt is null || debt.IsDeleted)
            throw new BusinessRuleException("Linked debt record was not found.");

        var delta = newAmount - oldAmount;
        if (txn.Type == TransactionType.DebtRepay)
        {
            var source = await RequireSourceAsync(db, txn.SourceId, cancellationToken).ConfigureAwait(false);
            if (delta > 0 && source.Balance < delta)
                throw new BusinessRuleException("Insufficient balance on the payment source.");
            source.Balance -= delta;
            debt.RemainingAmount -= delta;
        }
        else
        {
            var source = await RequireSourceAsync(db, txn.SourceId, cancellationToken).ConfigureAwait(false);
            source.Balance += delta;
            debt.RemainingAmount -= delta;
        }

        if (debt.RemainingAmount < 0)
            throw new BusinessRuleException("Payment amount would exceed the remaining debt balance.");

        if (debt.RemainingAmount == 0)
            debt.Status = DebtStatus.Completed;
        else if (debt.Status == DebtStatus.Completed)
            debt.Status = DebtStatus.Active;

        leg.Amount = newAmount;
    }

    private static async Task ApplyDebtOriginationAsync(
        IApplicationDbContext db,
        FinTransaction txn,
        decimal newAmount,
        decimal oldAmount,
        CancellationToken cancellationToken)
    {
        var debt = await db.FinDebtRecords
            .FirstOrDefaultAsync(d => d.OriginalTxnId == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        if (debt is null || debt.IsDeleted)
            throw new BusinessRuleException("Linked debt record was not found.");

        var delta = newAmount - oldAmount;
        var source = await RequireSourceAsync(db, txn.SourceId, cancellationToken).ConfigureAwait(false);

        if (txn.Type == TransactionType.LoanGive)
        {
            if (delta > 0 && source.Balance < delta)
                throw new BusinessRuleException("Insufficient balance on the source.");
            source.Balance -= delta;
        }
        else
        {
            source.Balance += delta;
        }

        debt.OriginalAmount += delta;
        debt.RemainingAmount += delta;

        if (debt.RemainingAmount < 0)
            throw new BusinessRuleException("Amount would make remaining debt negative.");

        if (debt.RemainingAmount == 0)
            debt.Status = DebtStatus.Completed;
    }

    private static async Task RecalculateDeferredBillingCycleAsync(
        IApplicationDbContext db,
        Guid transactionId,
        CancellationToken cancellationToken)
    {
        var item = await db.FinBillingCycleItems
            .FirstOrDefaultAsync(i => i.TransactionId == transactionId && i.RemovedAt == null, cancellationToken)
            .ConfigureAwait(false);

        if (item is null)
            return;

        var cycle = await db.FinBillingCycles
            .FirstOrDefaultAsync(c => c.Id == item.BillingCycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is not null && cycle.Status == BillingCycleStatus.Open)
            await BillingCycleTotals.RecalculateAsync(cycle, db, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<FinSource> RequireSourceAsync(
        IApplicationDbContext db,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var source = await db.FinSources
            .FirstOrDefaultAsync(s => s.Id == sourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived.");

        return source;
    }

    private static void EnsureSufficientBalance(
        FinSource source,
        TransactionType type,
        decimal oldAmount,
        decimal newAmount)
    {
        if (source.Type == SourceType.CreditCard)
            return;

        var change = TransactionBalanceLegMath.BalanceChange(type, oldAmount, newAmount);
        if (change < 0 && source.Balance + change < 0)
            throw new BusinessRuleException("Insufficient balance on the source.");
    }
}
