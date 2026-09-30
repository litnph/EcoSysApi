using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReportAddableTransactions;

public sealed class GetMonthlyReportAddableTransactionsQueryHandler
    : IRequestHandler<GetMonthlyReportAddableTransactionsQuery, GetMonthlyReportAddableTransactionsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMonthlyReportAddableTransactionsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetMonthlyReportAddableTransactionsResponse> Handle(
        GetMonthlyReportAddableTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var period = await _db.FinMonthlyPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Year == request.Year && item.Month == request.Month,
                cancellationToken)
            .ConfigureAwait(false);

        if (period is null || period.ReportCreatedAt is null)
            throw new NotFoundException("Monthly report was not found.");

        if (period.Status != PeriodStatus.Open)
            return new GetMonthlyReportAddableTransactionsResponse([]);

        var ownedTransactionIds = await MonthlyReportTransactionOwnership
            .GetOwnedTransactionIdsAsync(_db, cancellationToken)
            .ConfigureAwait(false);

        var items = await (
                from transaction in _db.FinTransactions.AsNoTracking()
                join source in _db.FinSources.AsNoTracking() on transaction.SourceId equals source.Id
                join category in _db.FinCategories.AsNoTracking()
                    on transaction.CategoryId equals category.Id into categoryJoin
                from category in categoryJoin.DefaultIfEmpty()
                where !transaction.IsDeleted
                      && transaction.Type == TransactionType.Direct
                      && source.Type != SourceType.CreditCard
                      && transaction.Purpose != TransactionPurpose.StatementPayment
                      && transaction.Note != BillingCyclePaymentNotes.StatementPayment
                      && transaction.Description != BillingCyclePaymentNotes.StatementPayment
                      && transaction.MonthlyPeriodId == null
                      && !ownedTransactionIds.Contains(transaction.Id)
                orderby transaction.TxnDate descending, transaction.CreatedAt descending
                select new TransactionListItemDto(
                    transaction.Id,
                    transaction.Type,
                    transaction.Purpose,
                    transaction.Status,
                    (long)Math.Round(transaction.Amount, 0, MidpointRounding.AwayFromZero),
                    transaction.Currency,
                    transaction.TxnDate,
                    transaction.SourceId,
                    source.Name,
                    transaction.CategoryId,
                    category != null ? category.Name : null,
                    transaction.Description,
                    transaction.Note,
                    transaction.CreatedAt,
                    false,
                    false,
                    null,
                    Array.Empty<TransactionTagDto>()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetMonthlyReportAddableTransactionsResponse(items);
    }
}
