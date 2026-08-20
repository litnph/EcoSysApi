using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Entities.Finance;
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

        var nextCurrency = request.Currency.Trim().ToUpperInvariant();
        if (entity.InvestmentTxns.Count > 0
            && !string.Equals(entity.Currency, nextCurrency, StringComparison.Ordinal))
            throw new BusinessRuleException("Investment currency is immutable after the first ledger event.");

        var valuationChanged = entity.CurrentValue != request.CurrentValue;
entity.Name = request.Name.Trim();
        entity.Type = request.Type;
        entity.CurrentValue = request.CurrentValue;
        entity.Currency = nextCurrency;
        entity.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        if (valuationChanged)
        {
            _db.FinInvestmentTxns.Add(new FinInvestmentTxn
            {
                InvestmentId = entity.Id,
                TxnType = InvestmentTxnType.Valuation,
                Amount = request.CurrentValue,
                TxnDate = FinanceBusinessCalendar.Today,
                Note = $"Valuation ({InvestmentDtoMapper.ProfitLossFormulaVersion})",
            });
        }
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        var txnRows = await _db.FinInvestmentTxns
            .AsNoTracking()
            .Where(t => t.InvestmentId == entity.Id)
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var txns = txnRows.ConvertAll(InvestmentDtoMapper.ToTxnDto);

        return new UpdateInvestmentResponse(InvestmentDtoMapper.ToDetail(entity, txns));
    }
}
