using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.GetSourceBalanceLedger;

public sealed class GetSourceBalanceLedgerQueryHandler
    : IRequestHandler<GetSourceBalanceLedgerQuery, GetSourceBalanceLedgerResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSourceBalanceLedgerQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetSourceBalanceLedgerResponse> Handle(
        GetSourceBalanceLedgerQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var source = await _db.FinSources
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null)
            throw new NotFoundException("Source was not found.");

        if (!AssetSourceRules.SupportsBalanceLedger(source.Type))
            throw new BusinessRuleException("Balance ledger is only available for non-credit-card sources.");

        var txns = await _db.FinTransactions
            .AsNoTracking()
            .Where(t => t.SourceId == source.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var (entries, computed) = SourceBalanceLedgerBuilder.Build(source, txns);
        var stored = source.Balance;
        var drift = stored - computed;

        return new GetSourceBalanceLedgerResponse(
            source.Id,
            source.Name,
            source.Currency,
            CurrencyUnits.ToWhole(stored),
            CurrencyUnits.ToWhole(computed),
            CurrencyUnits.ToWhole(drift),
            entries);
    }
}
