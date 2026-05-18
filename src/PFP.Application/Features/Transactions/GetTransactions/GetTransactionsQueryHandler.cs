using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactions;

/// <summary>Projects filtered transactions for list screens.</summary>
public sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, GetTransactionsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetTransactionsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns a flat page ordered by business date then creation time.</summary>
    /// <inheritdoc cref="IRequestHandler{GetTransactionsQuery, GetTransactionsResponse}.Handle" />
    public async Task<GetTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read transactions for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var filtered = _db.FinTransactions
            .AsNoTracking()
            .Where(t => t.SmoduleId == request.SmoduleId);

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
        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        if (totalPages == 0)
            totalPages = 1;

        var pageQuery =
            from t in filtered
            join s in _db.FinSources.AsNoTracking() on t.SourceId equals s.Id
            join c in _db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id into cj
            from c in cj.DefaultIfEmpty()
            orderby t.TxnDate descending, t.CreatedAt descending
            select new TransactionListItemDto(
                t.Id,
                t.SmoduleId,
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
                t.CreatedAt);

        var items = await pageQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetTransactionsResponse(items, request.Page, request.PageSize, total, totalPages);
    }
}
