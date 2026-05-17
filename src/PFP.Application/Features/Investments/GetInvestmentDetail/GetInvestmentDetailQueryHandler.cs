using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.GetInvestmentDetail;

public sealed class GetInvestmentDetailQueryHandler : IRequestHandler<GetInvestmentDetailQuery, GetInvestmentDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetInvestmentDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetInvestmentDetailResponse> Handle(GetInvestmentDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinInvestments
            .AsNoTracking()
            .Include(i => i.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Investment was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to read investments for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this investment.");

        var txns = await _db.FinInvestmentTxns
            .AsNoTracking()
            .Where(t => t.InvestmentId == entity.Id)
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtoTxns = txns.ConvertAll(InvestmentDtoMapper.ToTxnDto);

        return new GetInvestmentDetailResponse(InvestmentDtoMapper.ToDetail(entity, dtoTxns));
    }
}
