using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Savings.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.CreateSaving;

public sealed class CreateSavingCommandHandler : IRequestHandler<CreateSavingCommand, CreateSavingResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateSavingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateSavingResponse> Handle(CreateSavingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived || source.Type == SourceType.CreditCard)
            throw new BusinessRuleException("Savings require an active non-credit-card source.");

        if (request.Status != SavingStatus.Active)
            throw new BusinessRuleException("New savings records must start in the active status.");

        if (request.MaturityDate is { } maturityDate && maturityDate < request.StartDate)
            throw new BusinessRuleException("Maturity date cannot be earlier than start date.");

        var entity = new FinSaving
        {            SourceId = request.SourceId,
            Name = request.Name.Trim(),
            TargetAmount = request.TargetAmount is { } target ? CurrencyUnits.FromWhole(target) : null,
            CurrentAmount = 0,
            InterestRate = request.InterestRate,
            StartDate = request.StartDate,
            MaturityDate = request.MaturityDate,
            Type = request.Type,
            Status = request.Status,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinSavings.Add(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new CreateSavingResponse(SavingDtoMapper.ToDetail(entity, source.Name));
    }
}
