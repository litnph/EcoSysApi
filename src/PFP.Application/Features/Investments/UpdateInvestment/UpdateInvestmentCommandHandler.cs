using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.UpdateInvestment;

public sealed class UpdateInvestmentCommandHandler : IRequestHandler<UpdateInvestmentCommand, UpdateInvestmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateInvestmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UpdateInvestmentResponse> Handle(UpdateInvestmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinInvestments
            .Include(i => i.InvestmentTxns)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Investment was not found.");
entity.Name = request.Name.Trim();
        entity.Type = request.Type;
        entity.CurrentValue = request.CurrentValue;
        entity.Currency = request.Currency.Trim().ToUpperInvariant();
        entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        var txns = entity.InvestmentTxns
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(InvestmentDtoMapper.ToTxnDto)
            .ToList();

        return new UpdateInvestmentResponse(InvestmentDtoMapper.ToDetail(entity, txns));
    }
}
