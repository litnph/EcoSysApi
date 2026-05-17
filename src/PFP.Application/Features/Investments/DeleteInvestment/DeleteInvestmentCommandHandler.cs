using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.DeleteInvestment;

public sealed class DeleteInvestmentCommandHandler : IRequestHandler<DeleteInvestmentCommand, DeleteInvestmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteInvestmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DeleteInvestmentResponse> Handle(DeleteInvestmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinInvestments
            .Include(i => i.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Investment was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage investments for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this investment.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinInvestments.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteInvestmentResponse(request.Id);
    }
}
