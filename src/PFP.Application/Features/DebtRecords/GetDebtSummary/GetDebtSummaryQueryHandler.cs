using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtSummary;

public sealed class GetDebtSummaryQueryHandler : IRequestHandler<GetDebtSummaryQuery, GetDebtSummaryResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDebtSummaryQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetDebtSummaryResponse> Handle(GetDebtSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeBorrowed = await _db.FinDebtRecords.AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var activeLent = await _db.FinDebtRecords.AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var totalBorrowedRemaining = activeBorrowed.Sum(r => r.RemainingAmount);
        var totalLentRemaining = activeLent.Sum(r => r.RemainingAmount);

        var overdueBorrowedCount = activeBorrowed.Count(r =>
            r.DueDate is { } d && d < today);

        var overdueLentCount = activeLent.Count(r =>
            r.DueDate is { } d && d < today);

        return new GetDebtSummaryResponse(
            CurrencyUnits.ToWhole(totalBorrowedRemaining),
            CurrencyUnits.ToWhole(totalLentRemaining),
            overdueBorrowedCount,
            overdueLentCount);
    }
}
