using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;

namespace PFP.Application.Features.Sources.GetSourcesRecalculatePreview;

public sealed class GetSourcesRecalculatePreviewQueryHandler
    : IRequestHandler<GetSourcesRecalculatePreviewQuery, GetSourcesRecalculatePreviewResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IBalanceCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public GetSourcesRecalculatePreviewQueryHandler(
        IApplicationDbContext db,
        IBalanceCalculator calculator,
        ICurrentUserService currentUser)
    {
        _db = db;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<GetSourcesRecalculatePreviewResponse> Handle(
        GetSourcesRecalculatePreviewQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var sources = await _db.FinSources
            .AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var installmentBySource = await SourceInstallmentMetrics
            .GetRemainingBySourceIdAsync(_db, cancellationToken)
            .ConfigureAwait(false);

        var items = new List<SourceRecalculatePreviewItemDto>(sources.Count);
        foreach (var source in sources)
        {
            var computed = await _calculator
                .PreviewAsync(source.Id, cancellationToken)
                .ConfigureAwait(false);

            var storedWhole = CurrencyUnits.ToWhole(source.Balance);
            var computedWhole = CurrencyUnits.ToWhole(computed);
            var limitWhole = source.CreditLimit is { } limit
                ? CurrencyUnits.ToWhole(limit)
                : (long?)null;
            var installmentRemaining = installmentBySource.GetValueOrDefault(source.Id, 0);

            items.Add(new SourceRecalculatePreviewItemDto(
                source.Id,
                source.Name,
                source.Type,
                source.Currency,
                storedWhole,
                computedWhole,
                computedWhole - storedWhole,
                limitWhole,
                installmentRemaining,
                CreditCardBalanceRules.UtilizationPercent(storedWhole, limitWhole),
                CreditCardBalanceRules.UtilizationPercent(computedWhole, limitWhole)));
        }

        return new GetSourcesRecalculatePreviewResponse(items);
    }
}
