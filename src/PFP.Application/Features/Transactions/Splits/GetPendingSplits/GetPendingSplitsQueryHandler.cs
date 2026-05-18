using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.GetPendingSplits;

/// <summary>Pending splits for a module, grouped by transaction, newest groups first.</summary>
public sealed class GetPendingSplitsQueryHandler : IRequestHandler<GetPendingSplitsQuery, GetPendingSplitsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetPendingSplitsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetPendingSplitsResponse> Handle(GetPendingSplitsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read this module.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var pending = await _db.FinTxnSplits
            .AsNoTracking()
            .Include(s => s.Transaction)
            .Where(s => s.Transaction.SmoduleId == request.SmoduleId && s.Status == SplitStatus.Pending)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var groups = pending
            .GroupBy(s => s.TransactionId)
            .OrderByDescending(g => g.Select(x => x.Transaction.CreatedAt).First())
            .Select(g =>
            {
                var head = g.First().Transaction;
                var splits = g
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new TxnSplitListDto(
                        s.Id,
                        s.PersonName,
                        s.PersonContact,
                        CurrencyUnits.ToWhole(s.Amount),
                        s.Status,
                        s.CreatedAt))
                    .ToList();

                return new PendingSplitGroupDto(
                    g.Key,
                    head.TxnDate,
                    CurrencyUnits.ToWhole(head.Amount),
                    head.Currency,
                    head.Description,
                    splits);
            })
            .ToList();

        return new GetPendingSplitsResponse(groups);
    }
}
