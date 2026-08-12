using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Protects closed accounting periods from new or in-place posting mutations.</summary>
public static class PostingPeriodPolicy
{
    public static async Task EnsureOpenTargetAsync(
        IApplicationDbContext db,
        DateOnly txnDate,
        Guid? monthlyPeriodId,
        CancellationToken cancellationToken = default)
    {
        var datePeriod = await db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Year == txnDate.Year && p.Month == txnDate.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (datePeriod?.Status == PeriodStatus.Closed)
            throw new BusinessRuleException("Transactions cannot be posted into a closed monthly period.");

        if (monthlyPeriodId is not { } periodId)
            return;

        var selectedPeriod = await db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            .ConfigureAwait(false);

        if (selectedPeriod is null)
            throw new NotFoundException("Monthly period was not found for this module.");

        if (selectedPeriod.Status != PeriodStatus.Open)
            throw new BusinessRuleException("Transactions cannot be assigned to a closed monthly period.");

        if (selectedPeriod.Year != txnDate.Year || selectedPeriod.Month != txnDate.Month)
            throw new BusinessRuleException("Transaction date must belong to the selected monthly period.");
    }

    public static async Task EnsureExistingTransactionMutableAsync(
        IApplicationDbContext db,
        FinTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (transaction.Status == TxnStatus.Completed)
            throw new BusinessRuleException("Completed transactions in a closed month are immutable.");

        var closedByDate = await db.FinMonthlyPeriods
            .AsNoTracking()
            .AnyAsync(
                p => p.Year == transaction.TxnDate.Year
                     && p.Month == transaction.TxnDate.Month
                     && p.Status == PeriodStatus.Closed,
                cancellationToken)
            .ConfigureAwait(false);

        if (closedByDate)
            throw new BusinessRuleException("Transactions in a closed monthly period are immutable.");

        if (transaction.MonthlyPeriodId is not { } periodId)
            return;

        var periodClosed = await db.FinMonthlyPeriods
            .AsNoTracking()
            .AnyAsync(p => p.Id == periodId && p.Status == PeriodStatus.Closed, cancellationToken)
            .ConfigureAwait(false);

        if (periodClosed)
            throw new BusinessRuleException("Transactions assigned to a closed monthly period are immutable.");
    }
}
