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
            .Include(s => s.Source)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Savings record was not found.");
var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
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

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new UpdateSavingResponse(SavingDtoMapper.ToDetail(entity, source.Name));
    }
}
