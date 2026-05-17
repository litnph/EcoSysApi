using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Utils;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.WithdrawFromSaving;

public sealed class WithdrawFromSavingCommandHandler : IRequestHandler<WithdrawFromSavingCommand, WithdrawFromSavingResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public WithdrawFromSavingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<WithdrawFromSavingResponse> Handle(WithdrawFromSavingCommand request, CancellationToken cancellationToken)
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

        if (saving.Status is SavingStatus.Withdrawn)
            throw new BusinessRuleException("This savings record is already marked as withdrawn.");

        if (saving.CurrentAmount < request.Amount)
            throw new BusinessRuleException("The savings balance is lower than the withdrawal amount.");

        var source = saving.Source;
        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        if (request.MonthlyPeriodId is { } mpId)
        {
            var mpOk = await _db.FinMonthlyPeriods
                .AnyAsync(p => p.Id == mpId && p.SmoduleId == saving.SmoduleId, cancellationToken)
                .ConfigureAwait(false);
            if (!mpOk)
                throw new NotFoundException("Monthly period was not found for this module.");
        }

        var description = Truncate512($"Savings withdrawal: {saving.Name}");
        var externalRef = $"saving:{saving.Id}";

        var txn = new FinTransaction
        {
            SmoduleId = saving.SmoduleId,
            Type = TransactionType.Transfer,
            Status = TxnStatus.Completed,
            Amount = request.Amount,
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
        source.Balance += request.Amount;
        saving.CurrentAmount -= request.Amount;

        FinTransactionHistoryHelper.AddCreated(_db, _currentUser, txn);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txn.Id, cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawFromSavingResponse(MapDetail(persisted));
    }

    private static string Truncate512(string text) => text.Length <= 512 ? text : text[..512];

    private static TransactionDetailDto MapDetail(FinTransaction t)
    {
        TransactionSourceSummaryDto? src = t.Source is null
            ? null
            : new TransactionSourceSummaryDto(t.Source.Id, t.Source.Name, t.Source.Currency, t.Source.Balance);

        TransactionCategorySummaryDto? cat = t.Category is null
            ? null
            : new TransactionCategorySummaryDto(t.Category.Id, t.Category.Name, t.Category.Kind);

        return new TransactionDetailDto(
            t.Id,
            t.SmoduleId,
            t.Type,
            t.Status,
            t.Amount,
            t.Currency,
            t.TxnDate,
            t.SourceId,
            t.CategoryId,
            t.Description,
            t.Note,
            t.BillingCycleId,
            t.MonthlyPeriodId,
            t.RefTxnId,
            t.CreatedAt,
            t.UpdatedAt,
            t.Version,
            src,
            cat);
    }
}
