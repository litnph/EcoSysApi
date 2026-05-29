using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Sources.RecalculateSourceBalance;

public sealed class RecalculateSourceBalanceCommandHandler
    : IRequestHandler<RecalculateSourceBalanceCommand, RecalculateSourceBalanceResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IBalanceCalculator _calculator;
    private readonly ICurrentUserService _currentUser;

    public RecalculateSourceBalanceCommandHandler(
        IApplicationDbContext db,
        IBalanceCalculator calculator,
        ICurrentUserService currentUser)
    {
        _db = db;
        _calculator = calculator;
        _currentUser = currentUser;
    }

    public async Task<RecalculateSourceBalanceResponse> Handle(
        RecalculateSourceBalanceCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null)
            throw new NotFoundException("Source was not found.");

        var previous = CurrencyUnits.ToWhole(source.Balance);
        var computed = await _calculator.RecalculateAsync(request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        return new RecalculateSourceBalanceResponse(
            source.Id,
            previous,
            CurrencyUnits.ToWhole(computed));
    }
}
