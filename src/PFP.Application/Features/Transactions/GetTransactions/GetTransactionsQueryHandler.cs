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

        var filtered = _db.FinTransactions.AsNoTracking();

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

        var total = await filtered.CountAsync(cancellationToken).ConfigureAwait(false);
        var totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)request.PageSize);

        var items = await (
            from t in filtered
            join s in _db.FinSources.AsNoTracking() on t.SourceId equals s.Id
            join c in _db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
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
                t.CreatedAt))
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetTransactionsResponse(items, request.Page, request.PageSize, total, totalPages);
    }
}
