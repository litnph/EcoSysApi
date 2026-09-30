using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Application.Features.MonthlyPeriods.RefreshMonthlyReport;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.AddMonthlyReportTransaction;

public sealed class AddMonthlyReportTransactionCommandHandler
    : IRequestHandler<AddMonthlyReportTransactionCommand, RefreshMonthlyReportResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddMonthlyReportTransactionCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefreshMonthlyReportResponse> Handle(
        AddMonthlyReportTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var period = await _db.FinMonthlyPeriods
                .FirstOrDefaultAsync(
                    item => item.Year == request.Year && item.Month == request.Month,
                    ct)
                .ConfigureAwait(false);

            if (period is null || period.ReportCreatedAt is null)
                throw new NotFoundException("Monthly report was not found.");

            if (period.Status != PeriodStatus.Open)
                throw new BusinessRuleException("Only an open monthly report can be edited.");

            var transaction = await _db.FinTransactions
                .Include(item => item.Source)
                .FirstOrDefaultAsync(item => item.Id == request.TransactionId, ct)
                .ConfigureAwait(false);

            if (transaction is null || transaction.IsDeleted)
                throw new NotFoundException("Transaction was not found.");

            var ownedTransactionIds = await MonthlyReportTransactionOwnership
                .GetOwnedTransactionIdsAsync(_db, ct)
                .ConfigureAwait(false);
            var isEligible = transaction.Type == TransactionType.Direct
                             && transaction.Source.Type != SourceType.CreditCard
                             && transaction.Purpose != TransactionPurpose.StatementPayment
                             && transaction.Note != BillingCyclePaymentNotes.StatementPayment
                             && transaction.Description != BillingCyclePaymentNotes.StatementPayment
                             && transaction.MonthlyPeriodId is null
                             && !ownedTransactionIds.Contains(transaction.Id);
            if (!isEligible)
            {
                throw new BusinessRuleException(
                    "Transaction cannot be added: it must be a direct-source transaction that is not in any monthly report.");
            }

            transaction.MonthlyPeriodId = period.Id;
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            var userId = _currentUser.UserId.Value;
            var preferences = await MonthlyReportUserPreferences
                .GetAsync(_db, userId, ct)
                .ConfigureAwait(false);
            var report = await MonthlyPeriodSummaryCalculator
                .BuildReportAsync(
                    _db,
                    request.Year,
                    request.Month,
                    userId,
                    preferences.ReportDay,
                    preferences.PeriodMode,
                    ct,
                    new MonthlyReportTransactionOwnership.ReportIdentity(
                        period.Id,
                        period.ReportCreatedAt.Value))
                .ConfigureAwait(false);

            await MonthlyReportTransactionOwnership
                .SynchronizeAsync(_db, period.Id, report, ct)
                .ConfigureAwait(false);
            MonthlyReportPeriodWriter.Apply(period, report, DateTime.UtcNow);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return new RefreshMonthlyReportResponse(report);
        }, cancellationToken).ConfigureAwait(false);
    }
}
