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
        var today = FinanceBusinessCalendar.Today;

        var totalBorrowedRemaining = await _db.FinDebtRecords
            .AsNoTracking()
            .Where(r => r.Direction == DebtDirection.Borrowed && r.Status == DebtStatus.Active)
            .SumAsync(r => (decimal?)r.RemainingAmount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var totalLentRemaining = await _db.FinDebtRecords
            .AsNoTracking()
            .Where(r => r.Direction == DebtDirection.Lent && r.Status == DebtStatus.Active)
            .SumAsync(r => (decimal?)r.RemainingAmount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var overdueBorrowedCount = await _db.FinDebtRecords
            .AsNoTracking()
            .Where(r =>
                r.Direction == DebtDirection.Borrowed
                && r.Status == DebtStatus.Active
                && r.DueDate != null
                && r.DueDate < today)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        var overdueLentCount = await _db.FinDebtRecords
            .AsNoTracking()
            .Where(r =>
                r.Direction == DebtDirection.Lent
                && r.Status == DebtStatus.Active
                && r.DueDate != null
                && r.DueDate < today)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetDebtSummaryResponse(
            CurrencyUnits.ToWhole(totalBorrowedRemaining),
            CurrencyUnits.ToWhole(totalLentRemaining),
            overdueBorrowedCount,
            overdueLentCount);
    }
}
