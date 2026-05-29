using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Sources.ApplySourcesRecalculate;

public sealed class ApplySourcesRecalculateCommandHandler
    : IRequestHandler<ApplySourcesRecalculateCommand, ApplySourcesRecalculateResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IBalanceCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public ApplySourcesRecalculateCommandHandler(
        IApplicationDbContext db,
        IBalanceCalculator calculator,
        ICurrentUserService currentUser)
    {
        _db = db;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<ApplySourcesRecalculateResponse> Handle(
        ApplySourcesRecalculateCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (request.SourceIds.Count == 0)
            throw new BusinessRuleException("Chọn ít nhất một nguồn để áp dụng.");

        var distinctIds = request.SourceIds.Distinct().ToList();
        var sources = await _db.FinSources
            .Where(s => distinctIds.Contains(s.Id))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (sources.Count != distinctIds.Count)
            throw new NotFoundException("One or more sources were not found.");

        var results = new List<ApplySourcesRecalculateResultItem>(sources.Count);
        foreach (var source in sources.OrderBy(s => s.Name))
        {
            var previous = CurrencyUnits.ToWhole(source.Balance);
            var computed = await _calculator
                .RecalculateAsync(source.Id, cancellationToken)
                .ConfigureAwait(false);
            var computedWhole = CurrencyUnits.ToWhole(computed);

            results.Add(new ApplySourcesRecalculateResultItem(
                source.Id,
                previous,
                computedWhole,
                previous != computedWhole));
        }

        return new ApplySourcesRecalculateResponse(results);
    }
}
