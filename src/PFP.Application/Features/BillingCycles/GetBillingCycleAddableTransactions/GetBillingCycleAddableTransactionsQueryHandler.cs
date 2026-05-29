using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleAddableTransactions;

public sealed class GetBillingCycleAddableTransactionsQueryHandler
    : IRequestHandler<GetBillingCycleAddableTransactionsQuery, GetBillingCycleAddableTransactionsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetBillingCycleAddableTransactionsQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetBillingCycleAddableTransactionsResponse> Handle(
        GetBillingCycleAddableTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var cycle = await _db.FinBillingCycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        if (cycle.Status != BillingCycleStatus.Open)
            return new GetBillingCycleAddableTransactionsResponse([]);

        var activeTxnIds = _db.FinBillingCycleItems
            .AsNoTracking()
            .Where(i => i.RemovedAt == null)
            .Select(i => i.TransactionId);

        var items = await (
            from t in _db.FinTransactions.AsNoTracking()
            join s in _db.FinSources.AsNoTracking() on t.SourceId equals s.Id
            join c in _db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            where !t.IsDeleted
                  && t.Type == TransactionType.Deferred
                  && t.Status == TxnStatus.New
                  && t.SourceId == cycle.SourceId
                  && t.TxnDate <= cycle.StatementDate
                  && !activeTxnIds.Contains(t.Id)
            orderby t.TxnDate descending, t.CreatedAt descending
            select new TransactionListItemDto(
                t.Id,
                t.Type,
                t.Status,
                (long)Math.Round(t.Amount, 0, MidpointRounding.AwayFromZero),
                t.Currency,
                t.TxnDate,
                t.SourceId,
                s.Name,
                t.CategoryId,
                c != null ? c.Name : null,
                t.Description,
                t.Note,
                t.CreatedAt,
                _db.FinInstallmentPlans.Any(p => p.OriginalTxnId == t.Id),
                t.InstallmentPlanId != null,
                Array.Empty<TransactionTagDto>()))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetBillingCycleAddableTransactionsResponse(items);
    }
}
