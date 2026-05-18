using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Utils;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.DepositToSaving;

public sealed class DepositToSavingCommandHandler : IRequestHandler<DepositToSavingCommand, DepositToSavingResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DepositToSavingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DepositToSavingResponse> Handle(DepositToSavingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var saving = await _db.FinSavings
            .Include(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .Include(s => s.Source)
            .FirstOrDefaultAsync(s => s.Id == request.SavingId, cancellationToken)
            .ConfigureAwait(false);

        if (saving is null)
            throw new NotFoundException("Savings record was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(saving.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage savings for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && saving.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this savings record.");

        if (saving.Status is not (SavingStatus.Active or SavingStatus.Matured))
            throw new BusinessRuleException("Deposits are only allowed while the savings record is active or matured.");

        var source = saving.Source;
        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        if (source.Balance < CurrencyUnits.FromWhole(request.Amount))
            throw new BusinessRuleException("Insufficient balance on the linked financial source.");

        if (request.MonthlyPeriodId is { } mpId)
        {
            var mpOk = await _db.FinMonthlyPeriods
                .AnyAsync(p => p.Id == mpId && p.SmoduleId == saving.SmoduleId, cancellationToken)
                .ConfigureAwait(false);
            if (!mpOk)
                throw new NotFoundException("Monthly period was not found for this module.");
        }

        var description = Truncate512($"Savings deposit: {saving.Name}");
        var externalRef = $"saving:{saving.Id}";

        var txn = new FinTransaction
        {
            SmoduleId = saving.SmoduleId,
            Type = TransactionType.Transfer,
            Status = TxnStatus.Completed,
            Amount = -CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = source.Id,
            DestSourceId = null,
            CategoryId = null,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            ExternalRef = externalRef.Length <= 255 ? externalRef : externalRef[..255],
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        _db.FinTransactions.Add(txn);
        source.Balance -= CurrencyUnits.FromWhole(request.Amount);
        saving.CurrentAmount += CurrencyUnits.FromWhole(request.Amount);

        FinTransactionHistoryHelper.AddCreated(_db, _currentUser, txn);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        return new DepositToSavingResponse(MapDetail(persisted));
    }

    private static string Truncate512(string text) => text.Length <= 512 ? text : text[..512];

    private static TransactionDetailDto MapDetail(FinTransaction t) => TransactionDtoMapper.ToDetail(t);
}
