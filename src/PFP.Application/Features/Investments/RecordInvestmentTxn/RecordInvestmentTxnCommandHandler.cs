using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Investments.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.RecordInvestmentTxn;

public sealed class RecordInvestmentTxnCommandHandler : IRequestHandler<RecordInvestmentTxnCommand, RecordInvestmentTxnResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RecordInvestmentTxnCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RecordInvestmentTxnResponse> Handle(RecordInvestmentTxnCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var investment = await _db.FinInvestments
            .Include(i => i.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(i => i.Id == request.InvestmentId, cancellationToken)
            .ConfigureAwait(false);

        if (investment is null)
            throw new NotFoundException("Investment was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(investment.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage investments for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && investment.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this investment.");

        if (request.LinkedTxnId is { } linkedId)
        {
            var linked = await _db.FinTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == linkedId && t.SmoduleId == investment.SmoduleId, cancellationToken)
                .ConfigureAwait(false);

            if (linked is null)
                throw new NotFoundException("Linked transaction was not found for this module.");

            if (!string.Equals(linked.Currency, investment.Currency, StringComparison.Ordinal))
                throw new BusinessRuleException("Linked transaction currency must match the investment currency.");
        }

        ApplyAggregates(investment, request.TxnType, request.Amount);

        var row = new FinInvestmentTxn
        {
            InvestmentId = investment.Id,
            TxnType = request.TxnType,
            Amount = request.Amount,
            Quantity = request.Quantity,
            PricePerUnit = request.PricePerUnit,
            TxnDate = request.TxnDate,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            LinkedTxnId = request.LinkedTxnId,
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.FinInvestmentTxns.Add(row);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var txns = await _db.FinInvestmentTxns
            .AsNoTracking()
            .Where(t => t.InvestmentId == investment.Id)
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dtoTxns = txns.ConvertAll(InvestmentDtoMapper.ToTxnDto);

        // Reload investment aggregates after save
        var refreshed = await _db.FinInvestments
            .AsNoTracking()
            .FirstAsync(i => i.Id == investment.Id, cancellationToken)
            .ConfigureAwait(false);

        return new RecordInvestmentTxnResponse(InvestmentDtoMapper.ToDetail(refreshed, dtoTxns));
    }

    private static void ApplyAggregates(FinInvestment inv, InvestmentTxnType txnType, decimal amount)
    {
        switch (txnType)
        {
            case InvestmentTxnType.Buy:
                inv.TotalInvested += amount;
                inv.CurrentValue += amount;
                break;
            case InvestmentTxnType.Sell:
                inv.TotalReturned += amount;
                inv.CurrentValue -= amount;
                break;
            case InvestmentTxnType.Dividend:
                inv.TotalReturned += amount;
                break;
            case InvestmentTxnType.Fee:
                inv.TotalInvested += amount;
                inv.CurrentValue -= amount;
                break;
            default:
                throw new BusinessRuleException("Unsupported investment transaction type.");
        }
    }
}
