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

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read investments for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var rows = await _db.FinInvestments
            .AsNoTracking()
            .Where(i => i.SmoduleId == request.SmoduleId)
            .OrderBy(i => i.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtos = rows.ConvertAll(InvestmentDtoMapper.ToListItem);
        return new GetInvestmentsResponse(dtos);
    }
}
