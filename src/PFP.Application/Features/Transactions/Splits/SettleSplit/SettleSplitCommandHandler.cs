using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Application.Features.Transactions.Splits.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.SettleSplit;

/// <summary>Creates an income transaction and marks the split settled.</summary>
public sealed class SettleSplitCommandHandler : IRequestHandler<SettleSplitCommand, SettleSplitResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SettleSplitCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<SettleSplitResponse> Handle(SettleSplitCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var split = await _db.FinTxnSplits
            .Include(s => s.Transaction)
            .FirstOrDefaultAsync(s => s.Id == request.SplitId, cancellationToken)
            .ConfigureAwait(false);

        if (split is null)
            throw new NotFoundException("Split was not found.");
if (split.Status != SplitStatus.Pending)
            throw new BusinessRuleException("Only pending splits can be settled.");

        var payAmount = request.Amount is { } requested
            ? CurrencyUnits.FromWhole(requested)
            : split.Amount;
        if (payAmount <= 0 || payAmount > split.Amount)
            throw new BusinessRuleException("Settlement amount must be greater than zero and must not exceed the split amount.");

        var paymentSource = await _db.FinSources
            .FirstOrDefaultAsync(
                s => s.Id == request.PaymentSourceId,
                cancellationToken)
            .ConfigureAwait(false);

        if (paymentSource is null || paymentSource.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (paymentSource.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        if (!string.Equals(paymentSource.Currency, split.Transaction.Currency, StringComparison.Ordinal))
            throw new BusinessRuleException("Payment source currency must match the original split transaction.");

        var incomeCategory = await _db.FinCategories
            .OrderBy(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (incomeCategory is null)
            throw new BusinessRuleException(
                "An income category is required to record reimbursement; create one in this module first.");

        var person = split.PersonName.Trim();
        var note = $"Hoàn tiền split từ {person}";
        if (note.Length > 500)
            note = note[..500];

        var description = BuildDirectIncomeDescription(incomeCategory.Name);

        var income = new FinTransaction
        {            Type = TransactionType.Income,
            Status = TxnStatus.Completed,
            Amount = payAmount,
            Currency = paymentSource.Currency,
            TxnDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SourceId = paymentSource.Id,
            CategoryId = incomeCategory.Id,
            Description = description,
            Note = note,
        };

        _db.FinTransactions.Add(income);
        paymentSource.Balance += payAmount;

        var now = DateTime.UtcNow;
        split.Status = SplitStatus.Settled;
        split.SettledAt = now;
        split.SettledTxnId = income.Id;

        var history = new FinTransactionHistory
        {
            TransactionId = income.Id,
            Version = 1,
            ChangedBy = _currentUser.UserId,
            SessionId = _currentUser.SessionId,
            ChangeType = HistoryChangeType.Created,
            ChangedFields = null,
            Snapshot = TransactionHistoryJson.BuildCreatedSnapshot(income),
            ChangeReason = null,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.FinTransactionHistory.Add(history);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var refreshed = await _db.FinTxnSplits
            .AsNoTracking()
            .FirstAsync(s => s.Id == split.Id, cancellationToken)
            .ConfigureAwait(false);

        var dto = new TxnSplitDto(
            refreshed.Id,
            refreshed.TransactionId,
            refreshed.PersonName,
            refreshed.PersonContact,
            CurrencyUnits.ToWhole(refreshed.Amount),
            refreshed.Status,
            refreshed.SettledAt,
            refreshed.SettledTxnId);

        return new SettleSplitResponse(income.Id, dto);
    }

    private static string BuildDirectIncomeDescription(string categoryName)
    {
        var text = $"Income: {categoryName}".Trim();
        return text.Length <= 512 ? text : text[..512];
    }
}
