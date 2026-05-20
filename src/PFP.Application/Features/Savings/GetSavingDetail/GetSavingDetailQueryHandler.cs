using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Savings.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.GetSavingDetail;

public sealed class GetSavingDetailQueryHandler : IRequestHandler<GetSavingDetailQuery, GetSavingDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSavingDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GetSavingDetailResponse> Handle(GetSavingDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSavings
            .AsNoTracking()
            .Include(s => s.Source)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Savings record was not found.");
return new GetSavingDetailResponse(SavingDtoMapper.ToDetail(entity, entity.Source!.Name));
    }
}
