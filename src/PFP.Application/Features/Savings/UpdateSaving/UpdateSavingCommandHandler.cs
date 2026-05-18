using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Savings.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.UpdateSaving;

public sealed class UpdateSavingCommandHandler : IRequestHandler<UpdateSavingCommand, UpdateSavingResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateSavingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UpdateSavingResponse> Handle(UpdateSavingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSavings
            .Include(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .Include(s => s.Source)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Savings record was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage savings for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this savings record.");

        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId && s.SmoduleId == entity.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        entity.SourceId = request.SourceId;
        entity.Name = request.Name.Trim();
        entity.TargetAmount = request.TargetAmount is { } target ? CurrencyUnits.FromWhole(target) : null;
        entity.InterestRate = request.InterestRate;
        entity.StartDate = request.StartDate;
        entity.MaturityDate = request.MaturityDate;
        entity.Type = request.Type;
        entity.Status = request.Status;
        entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateSavingResponse(SavingDtoMapper.ToDetail(entity, source.Name));
    }
}
