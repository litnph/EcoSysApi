using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Savings.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.GetSavings;

public sealed class GetSavingsQueryHandler : IRequestHandler<GetSavingsQuery, GetSavingsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSavingsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetSavingsResponse> Handle(GetSavingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var rows = await _db.FinSavings
            .AsNoTracking()
            .Include(s => s.Source)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.ConvertAll(s => SavingDtoMapper.ToListItem(s, s.Source!.Name));

        return new GetSavingsResponse(dtos);
    }
}
