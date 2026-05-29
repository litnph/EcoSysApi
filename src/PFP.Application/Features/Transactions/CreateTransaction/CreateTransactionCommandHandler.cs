using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.CreateTransaction;

/// <summary>Creates direct, income, transfer, deferred, or debt / loan transactions; updates balances; seeds history v1.</summary>
public sealed class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CreateTransactionResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateTransactionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>Persists the transaction(s), balance movement, and manual history rows in one DB transaction.</summary>
    /// <inheritdoc cref="IRequestHandler{CreateTransactionCommand, CreateTransactionResponse}.Handle" />
    public async Task<CreateTransactionResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        request = await NormalizeExpenseCommandAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Type is TransactionType.DebtBorrow
            or TransactionType.LoanGive
            or TransactionType.DebtRepay
            or TransactionType.LoanCollect)
            return await HandleDebtAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Type == TransactionType.Transfer)
            return await HandleTransferAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Type == TransactionType.Deferred)
            return await HandleDeferredAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Type == TransactionType.Split)
            return await HandleSplitAsync(request, cancellationToken).ConfigureAwait(false);

        return await HandleDirectOrIncomeAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureMonthlyPeriodExistsAsync(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (request.MonthlyPeriodId is not { } mpId)
            return;
        var mpExists = await _db.FinMonthlyPeriods
            .AnyAsync(p => p.Id == mpId, cancellationToken)
            .ConfigureAwait(false);
        if (!mpExists)
            throw new NotFoundException("Monthly period was not found for this module.");
    }

    private async Task<CreateTransactionResponse> HandleDebtAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        await EnsureMonthlyPeriodExistsAsync(request, cancellationToken).ConfigureAwait(false);

        return request.Type switch
        {
            TransactionType.DebtBorrow => await HandleDebtBorrowAsync(request, cancellationToken).ConfigureAwait(false),
            TransactionType.LoanGive => await HandleLoanGiveAsync(request, cancellationToken).ConfigureAwait(false),
            TransactionType.DebtRepay => await HandleDebtRepayAsync(request, cancellationToken).ConfigureAwait(false),
            TransactionType.LoanCollect => await HandleLoanCollectAsync(request, cancellationToken).ConfigureAwait(false),
            _ => throw new BusinessRuleException("Unsupported debt-related transaction type."),
        };
    }

    private async Task<CreateTransactionResponse> HandleDebtBorrowAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        var personName = request.PersonName!.Trim();
        var description = ResolveDescription(
            request.Description,
            $"Borrowed: {personName}");

        var txn = new FinTransaction
        {            Type = TransactionType.DebtBorrow,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = null,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);
        source.Balance += CurrencyUnits.FromWhole(request.Amount);

        var debt = new FinDebtRecord
        {            Direction = DebtDirection.Borrowed,
            PersonName = personName,
            PersonContact = string.IsNullOrWhiteSpace(request.PersonContact) ? null : request.PersonContact.Trim(),
            OriginalTxnId = txn.Id,
            OriginalAmount = CurrencyUnits.FromWhole(request.Amount),
            RemainingAmount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            DueDate = request.DueDate,
            Status = DebtStatus.Active,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinDebtRecords.Add(debt);

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleLoanGiveAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        var personName = request.PersonName!.Trim();
        var description = ResolveDescription(
            request.Description,
            $"Loan given: {personName}");

        var txn = new FinTransaction
        {            Type = TransactionType.LoanGive,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = null,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);
        source.Balance -= CurrencyUnits.FromWhole(request.Amount);

        var debt = new FinDebtRecord
        {            Direction = DebtDirection.Lent,
            PersonName = personName,
            PersonContact = string.IsNullOrWhiteSpace(request.PersonContact) ? null : request.PersonContact.Trim(),
            OriginalTxnId = txn.Id,
            OriginalAmount = CurrencyUnits.FromWhole(request.Amount),
            RemainingAmount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            DueDate = null,
            Status = DebtStatus.Active,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinDebtRecords.Add(debt);

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleDebtRepayAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var debt = await _db.FinDebtRecords
            .FirstOrDefaultAsync(
                d => d.Id == request.DebtRecordId!.Value,
                cancellationToken)
            .ConfigureAwait(false);

        if (debt is null || debt.IsDeleted)
            throw new NotFoundException("Debt record was not found.");

        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        var description = ResolveDescription(
            request.Description,
            $"Debt repayment: {debt.PersonName}");

        var txn = new FinTransaction
        {            Type = TransactionType.DebtRepay,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = null,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);
        source.Balance -= CurrencyUnits.FromWhole(request.Amount);

        var leg = new FinDebtTransaction
        {
            DebtRecordId = debt.Id,
            TxnId = txn.Id,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Type = DebtTxnType.Payment,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            TxnDate = request.TxnDate,
        };

        _db.FinDebtTransactions.Add(leg);

        debt.RemainingAmount -= CurrencyUnits.FromWhole(request.Amount);
        if (debt.RemainingAmount <= 0)
        {
            debt.RemainingAmount = 0;
            debt.Status = DebtStatus.Completed;
        }

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleLoanCollectAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var debt = await _db.FinDebtRecords
            .FirstOrDefaultAsync(
                d => d.Id == request.DebtRecordId!.Value,
                cancellationToken)
            .ConfigureAwait(false);

        if (debt is null || debt.IsDeleted)
            throw new NotFoundException("Debt record was not found.");

        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        var description = ResolveDescription(
            request.Description,
            $"Loan collected: {debt.PersonName}");

        var txn = new FinTransaction
        {            Type = TransactionType.LoanCollect,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = null,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);
        source.Balance += CurrencyUnits.FromWhole(request.Amount);

        var leg = new FinDebtTransaction
        {
            DebtRecordId = debt.Id,
            TxnId = txn.Id,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Type = DebtTxnType.Collection,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            TxnDate = request.TxnDate,
        };

        _db.FinDebtTransactions.Add(leg);

        debt.RemainingAmount -= CurrencyUnits.FromWhole(request.Amount);
        if (debt.RemainingAmount <= 0)
        {
            debt.RemainingAmount = 0;
            debt.Status = DebtStatus.Completed;
        }

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleDeferredAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var categoryId = request.CategoryId
            ?? throw new BusinessRuleException("Category is required for this transaction type.");

        var category = await _db.FinCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
            throw new NotFoundException("Category was not found.");

        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.Type != SourceType.CreditCard)
            throw new BusinessRuleException("Deferred transactions require a credit card source.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        await EnsureMonthlyPeriodExistsAsync(request, cancellationToken).ConfigureAwait(false);

        var description = ResolveDescription(
            request.Description,
            BuildDirectIncomeDescription(TransactionType.Direct, category.Name));

        var txn = new FinTransaction
        {            Type = TransactionType.Deferred,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = categoryId,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);

        source.Balance += CurrencyUnits.FromWhole(request.Amount);

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleSplitAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var categoryId = request.CategoryId
            ?? throw new BusinessRuleException("Category is required for this transaction type.");

        var category = await _db.FinCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
            throw new NotFoundException("Category was not found.");

        var splits = request.Splits
            ?? throw new BusinessRuleException("Splits are required for a split transaction.");

        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        await EnsureMonthlyPeriodExistsAsync(request, cancellationToken).ConfigureAwait(false);

        if (source.Balance < CurrencyUnits.FromWhole(request.Amount))
            throw new BusinessRuleException("Insufficient balance on the selected source.");

        var description = ResolveDescription(
            request.Description,
            BuildDirectIncomeDescription(TransactionType.Split, category.Name));

        var txn = new FinTransaction
        {            Type = TransactionType.Split,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = categoryId,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);
        source.Balance -= CurrencyUnits.FromWhole(request.Amount);

        foreach (var item in splits)
        {
            var split = new FinTxnSplit
            {
                TransactionId = txn.Id,
                PersonName = item.PersonName.Trim(),
                PersonContact = string.IsNullOrWhiteSpace(item.PersonContact) ? null : item.PersonContact.Trim(),
                Amount = CurrencyUnits.FromWhole(item.Amount),
                Status = SplitStatus.Pending,
            };
            _db.FinTxnSplits.Add(split);
        }

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleDirectOrIncomeAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var categoryId = request.CategoryId
            ?? throw new BusinessRuleException("Category is required for this transaction type.");

        var category = await _db.FinCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            .ConfigureAwait(false);

        if (category is null)
            throw new NotFoundException("Category was not found.");

        Guid txnId = default;
        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        var source = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (source is null || source.IsDeleted)
            throw new BusinessRuleException("The financial source is not available.");

        if (source.IsArchived)
            throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

        await EnsureMonthlyPeriodExistsAsync(request, cancellationToken).ConfigureAwait(false);

        if (request.Type == TransactionType.Direct && source.Balance < CurrencyUnits.FromWhole(request.Amount))
            throw new BusinessRuleException("Insufficient balance on the selected source.");

        var description = ResolveDescription(
            request.Description,
            BuildDirectIncomeDescription(request.Type, category.Name));

        var txn = new FinTransaction
        {            Type = request.Type,
            Status = TxnStatus.New,
            Amount = CurrencyUnits.FromWhole(request.Amount),
            Currency = source.Currency,
            TxnDate = request.TxnDate,
            SourceId = request.SourceId,
            CategoryId = categoryId,
            MonthlyPeriodId = request.MonthlyPeriodId,
            Description = description,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
        };

        _db.FinTransactions.Add(txn);

        if (request.Type == TransactionType.Income)
            source.Balance += CurrencyUnits.FromWhole(request.Amount);
        else
            source.Balance -= CurrencyUnits.FromWhole(request.Amount);

        AddCreatedHistory(txn);

        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            txnId = txn.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persisted = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == txnId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persisted));
    }

    private async Task<CreateTransactionResponse> HandleTransferAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var toSourceId = request.ToSourceId
            ?? throw new BusinessRuleException("ToSourceId is required for a transfer.");

        await EnsureMonthlyPeriodExistsAsync(request, cancellationToken).ConfigureAwait(false);

        var outboundId = await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var fromSource = await _db.FinSources
                .FirstOrDefaultAsync(s => s.Id == request.SourceId, ct)
                .ConfigureAwait(false);

            var toSource = await _db.FinSources
                .FirstOrDefaultAsync(s => s.Id == toSourceId, ct)
                .ConfigureAwait(false);

            if (fromSource is null || fromSource.IsDeleted)
                throw new BusinessRuleException("The financial source is not available.");

            if (toSource is null || toSource.IsDeleted)
                throw new BusinessRuleException("The destination financial source is not available.");

            if (fromSource.IsArchived)
                throw new BusinessRuleException("The financial source is archived and cannot receive new transactions.");

            if (toSource.IsArchived)
                throw new BusinessRuleException("The destination financial source is archived and cannot receive new transactions.");

            if (fromSource.Balance < CurrencyUnits.FromWhole(request.Amount))
                throw new BusinessRuleException("Insufficient balance on the selected source.");

            if (!string.Equals(fromSource.Currency, toSource.Currency, StringComparison.Ordinal))
                throw new BusinessRuleException("Both sources must use the same currency for a transfer.");

            var description = ResolveDescription(
                request.Description,
                BuildTransferDescription(fromSource.Name, toSource.Name));

            var outbound = new FinTransaction
            {
                Type = TransactionType.Transfer,
                Status = TxnStatus.New,
                Amount = -CurrencyUnits.FromWhole(request.Amount),
                Currency = fromSource.Currency,
                TxnDate = request.TxnDate,
                SourceId = request.SourceId,
                DestSourceId = toSourceId,
                CategoryId = null,
                MonthlyPeriodId = request.MonthlyPeriodId,
                Description = description,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                RefTxnId = null,
            };

            var inbound = new FinTransaction
            {
                Type = TransactionType.Transfer,
                Status = TxnStatus.New,
                Amount = CurrencyUnits.FromWhole(request.Amount),
                Currency = toSource.Currency,
                TxnDate = request.TxnDate,
                SourceId = toSourceId,
                DestSourceId = request.SourceId,
                CategoryId = null,
                MonthlyPeriodId = request.MonthlyPeriodId,
                Description = description,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                RefTxnId = null,
            };

            _db.FinTransactions.Add(outbound);
            _db.FinTransactions.Add(inbound);

            fromSource.Balance -= CurrencyUnits.FromWhole(request.Amount);
            toSource.Balance += CurrencyUnits.FromWhole(request.Amount);

            AddCreatedHistory(outbound);
            AddCreatedHistory(inbound);

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            outbound.RefTxnId = inbound.Id;
            inbound.RefTxnId = outbound.Id;

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return outbound.Id;
        }, cancellationToken).ConfigureAwait(false);

        var persistedOutbound = await _db.FinTransactions
            .AsNoTracking()
            .Include(t => t.Source)
            .Include(t => t.Category)
            .FirstAsync(t => t.Id == outboundId, cancellationToken)
            .ConfigureAwait(false);

        return new CreateTransactionResponse(MapDetail(persistedOutbound));
    }

    private void AddCreatedHistory(FinTransaction txn)
    {
        var now = DateTime.UtcNow;
        var history = new FinTransactionHistory
        {
            TransactionId = txn.Id,
            Version = 1,
            ChangedBy = _currentUser.UserId,
            SessionId = _currentUser.SessionId,
            ChangeType = HistoryChangeType.Created,
            ChangedFields = null,
            Snapshot = TransactionHistoryJson.BuildCreatedSnapshot(txn),
            ChangeReason = null,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.FinTransactionHistory.Add(history);
    }

    private static string TruncateDescription(string text) =>
        text.Length <= 512 ? text : text[..512];

    private static string ResolveDescription(string? custom, string fallback)
    {
        if (custom is null)
            return TruncateDescription(fallback);
        var trimmed = custom.Trim();
        return trimmed.Length > 0 ? TruncateDescription(trimmed) : string.Empty;
    }

    private static string BuildDirectIncomeDescription(TransactionType type, string categoryName)
    {
        var prefix = type == TransactionType.Income ? "Income" : "Expense";
        var text = $"{prefix}: {categoryName}".Trim();
        return text.Length <= 512 ? text : text[..512];
    }

    private static string BuildTransferDescription(string fromName, string toName)
    {
        var text = $"Transfer: {fromName} → {toName}";
        return text.Length <= 512 ? text : text[..512];
    }

    /// <summary>Chi tiêu (<c>direct</c>) trên thẻ tín dụng → <c>deferred</c> (ghi nợ thẻ; gắn kỳ sao kê qua Refresh).</summary>
    private async Task<CreateTransactionCommand> NormalizeExpenseCommandAsync(
        CreateTransactionCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Type != TransactionType.Direct)
            return request;

        var src = await _db.FinSources
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SourceId, cancellationToken)
            .ConfigureAwait(false);

        if (src is null || src.Type != SourceType.CreditCard)
            return request;

        return request with { Type = TransactionType.Deferred };
    }

    private static TransactionDetailDto MapDetail(FinTransaction t) => TransactionDtoMapper.ToDetail(t);
}
