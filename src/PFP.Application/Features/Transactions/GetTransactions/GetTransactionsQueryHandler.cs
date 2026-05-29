using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactions;

public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, GetTransactionsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTransactionsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAppException("Authentication is required.");

        var filtered = _db.FinTransactions
            .AsNoTracking()
            .Where(t => t.Type != TransactionType.Reversal);

        if (request.SourceId is { } sid)
            filtered = filtered.Where(t => t.SourceId == sid);
        if (request.Type is { } ty)
            filtered = filtered.Where(t => t.Type == ty);
        if (request.CategoryId is { } cid)
            filtered = filtered.Where(t => t.CategoryId == cid);
        if (request.DateFrom is { } df)
            filtered = filtered.Where(t => t.TxnDate >= df);
        if (request.DateTo is { } dt)
            filtered = filtered.Where(t => t.TxnDate <= dt);
        if (request.AmountMin is { } min)
            filtered = filtered.Where(t => t.Amount >= min);
        if (request.AmountMax is { } max)
            filtered = filtered.Where(t => t.Amount <= max);
        if (request.Status is { } st)
            filtered = filtered.Where(t => t.Status == st);

        var total = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)request.PageSize);

        var rows = await (
            from t in filtered
            join s in _db.FinSources.AsNoTracking() on t.SourceId equals s.Id
            join c in _db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            orderby t.TxnDate descending, t.CreatedAt descending
            select new
            {
                t.Id,
                t.Type,
                t.Status,
                Amount = (long)Math.Round(t.Amount, 0, MidpointRounding.AwayFromZero),
                t.Currency,
                t.TxnDate,
                t.SourceId,
                SourceName = s.Name,
                t.CategoryId,
                CategoryName = c != null ? c.Name : null,
                t.Description,
                t.Note,
                t.CreatedAt,
                HasInstallmentPlan = _db.FinInstallmentPlans.Any(p => p.OriginalTxnId == t.Id),
                IsInstallmentPayment = t.InstallmentPlanId != null,
            })
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var tagMap = await TransactionTagQueries
            .ForTransactionsMapAsync(_db, rows.Select(r => r.Id), cancellationToken)
            .ConfigureAwait(false);

        var items = rows.Select(r => new TransactionListItemDto(
            r.Id,
            r.Type,
            r.Status,
            r.Amount,
            r.Currency,
            r.TxnDate,
            r.SourceId,
            r.SourceName,
            r.CategoryId,
            r.CategoryName,
            r.Description,
            r.Note,
            r.CreatedAt,
            r.HasInstallmentPlan,
            r.IsInstallmentPayment,
            tagMap.TryGetValue(r.Id, out var tags) ? tags : Array.Empty<TransactionTagDto>())).ToList();

        return new GetTransactionsResponse(items, request.Page, request.PageSize, total, totalPages);
    }
}
