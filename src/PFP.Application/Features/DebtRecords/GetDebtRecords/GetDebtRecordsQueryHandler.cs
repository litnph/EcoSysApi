using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed class GetDebtRecordsQueryHandler : IRequestHandler<GetDebtRecordsQuery, GetDebtRecordsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDebtRecordsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetDebtRecordsResponse> Handle(GetDebtRecordsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.FinDebtRecords.AsNoTracking();

        if (request.Direction is { } dir)
            query = query.Where(r => r.Direction == dir);

        if (request.Status is { } st)
            query = query.Where(r => r.Status == st);

        var rows = await query
            .OrderBy(r => r.Status != DebtStatus.Active ? 1 : 0)
            .ThenBy(r => r.DueDate ?? DateOnly.MaxValue)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows.Select(r => new DebtRecordListItemDto(
            r.Id,
            r.Direction,
            r.PersonName,
            r.PersonContact,
            CurrencyUnits.ToWhole(r.OriginalAmount),
            CurrencyUnits.ToWhole(r.RemainingAmount),
            r.Currency,
            r.DueDate,
            r.Status,
            r.DueDate is { } due ? (int?)(due.DayNumber - today.DayNumber) : null,
            r.CreatedAt)).ToList();

        return new GetDebtRecordsResponse(items);
    }
}
