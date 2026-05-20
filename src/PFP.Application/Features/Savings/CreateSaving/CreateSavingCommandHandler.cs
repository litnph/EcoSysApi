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

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinSavings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CreateSavingResponse(SavingDtoMapper.ToDetail(entity, source.Name));
    }
}
