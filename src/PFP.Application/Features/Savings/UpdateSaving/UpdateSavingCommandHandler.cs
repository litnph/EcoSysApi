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

        if (source.IsArchived || source.Type == SourceType.CreditCard)
            throw new BusinessRuleException("Savings require an active non-credit-card source.");

        if (entity.CurrentAmount > 0m && request.SourceId != entity.SourceId)
            throw new BusinessRuleException("The linked source is immutable while the savings record has funds.");

        if (!string.Equals(source.Currency, entity.Source.Currency, StringComparison.Ordinal))
            throw new BusinessRuleException("A savings source change must preserve currency.");

        if (request.MaturityDate is { } maturityDate && maturityDate < request.StartDate)
            throw new BusinessRuleException("Maturity date cannot be earlier than start date.");

        if (request.Status == SavingStatus.Withdrawn && entity.CurrentAmount != 0m)
            throw new BusinessRuleException("A savings record can be closed only when its current amount is zero.");

        var today = FinanceBusinessCalendar.Today;
        var derivedOpenStatus = request.MaturityDate is { } due && due <= today
            ? SavingStatus.Matured
            : SavingStatus.Active;
        var status = request.Status == SavingStatus.Withdrawn
            ? SavingStatus.Withdrawn
            : derivedOpenStatus;

        entity.SourceId = request.SourceId;
        entity.Name = request.Name.Trim();
        entity.TargetAmount = request.TargetAmount is { } target ? CurrencyUnits.FromWhole(target) : null;
        entity.InterestRate = request.InterestRate;
        entity.StartDate = request.StartDate;
        entity.MaturityDate = request.MaturityDate;
        entity.Type = request.Type;
        entity.Status = status;
        entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new UpdateSavingResponse(SavingDtoMapper.ToDetail(entity, source.Name));
    }
}
