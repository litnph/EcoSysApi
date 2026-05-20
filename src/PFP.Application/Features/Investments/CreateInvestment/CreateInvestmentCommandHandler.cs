using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.CreateInvestment;

public sealed class CreateInvestmentCommandHandler : IRequestHandler<CreateInvestmentCommand, CreateInvestmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateInvestmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateInvestmentResponse> Handle(CreateInvestmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");
var currency = string.IsNullOrWhiteSpace(request.Currency) ? "VND" : request.Currency.Trim().ToUpperInvariant();

        var entity = new FinInvestment
        {            Name = request.Name.Trim(),
            Type = request.Type,
            CurrentValue = 0,
            TotalInvested = 0,
            TotalReturned = 0,
            Currency = currency,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinInvestments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CreateInvestmentResponse(InvestmentDtoMapper.ToDetail(entity, Array.Empty<InvestmentTxnDto>()));
    }
}
