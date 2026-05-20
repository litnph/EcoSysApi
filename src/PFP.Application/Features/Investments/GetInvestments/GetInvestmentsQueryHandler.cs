using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.GetInvestments;

public sealed class GetInvestmentsQueryHandler : IRequestHandler<GetInvestmentsQuery, GetInvestmentsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetInvestmentsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetInvestmentsResponse> Handle(GetInvestmentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var rows = await _db.FinInvestments
            .AsNoTracking()
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.ConvertAll(InvestmentDtoMapper.ToListItem);
        return new GetInvestmentsResponse(dtos);
    }
}
