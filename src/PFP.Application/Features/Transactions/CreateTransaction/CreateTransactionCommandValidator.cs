using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.CreateTransaction;

/// <summary>FluentValidation rules for <see cref="CreateTransactionCommand"/>.</summary>
public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    private static readonly TransactionType[] DebtTypes =
    {
        TransactionType.DebtBorrow,
        TransactionType.LoanGive,
        TransactionType.DebtRepay,
        TransactionType.LoanCollect,
    };

    /// <summary>Registers validation rules.</summary>
    public CreateTransactionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Type)
            .Must(t => t is TransactionType.Direct
                       or TransactionType.Income
                       or TransactionType.Transfer
                       or TransactionType.Deferred
                       or TransactionType.Split
                       or TransactionType.DebtBorrow
                       or TransactionType.LoanGive
                       or TransactionType.DebtRepay
                       or TransactionType.LoanCollect)
            .WithMessage("This transaction type is not supported.");

        RuleFor(x => x.TxnDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
            .WithMessage("TxnDate cannot be more than one day in the future.");

        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
        RuleFor(x => x.PersonName).MaximumLength(200).When(x => x.PersonName is not null);
        RuleFor(x => x.PersonContact).MaximumLength(200).When(x => x.PersonContact is not null);

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .When(x => x.Type is TransactionType.Direct or TransactionType.Income or TransactionType.Deferred
                       or TransactionType.Split)
            .WithMessage("Category is required for this transaction type.");

        RuleFor(x => x.CategoryId)
            .Must(c => !c.HasValue)
            .When(x => x.Type is TransactionType.Transfer or TransactionType.DebtBorrow or TransactionType.LoanGive
                       or TransactionType.DebtRepay or TransactionType.LoanCollect)
            .WithMessage("Category must not be set for this transaction type.");

        RuleFor(x => x.BillingCycleId)
            .Must(c => !c.HasValue)
            .When(x => x.Type is not TransactionType.Deferred)
            .WithMessage("BillingCycleId is only used for deferred transactions.");

        RuleFor(x => x.ToSourceId)
            .Must(c => !c.HasValue)
            .When(x => x.Type is not TransactionType.Transfer)
            .WithMessage("ToSourceId is only used for transfers.");

        RuleFor(x => x.Splits)
            .Must(s => s is null)
            .When(x => x.Type != TransactionType.Split)
            .WithMessage("Splits are only used for split transactions.");

        RuleFor(x => x.Splits)
            .NotNull()
            .Must(s => s!.Count > 0)
            .When(x => x.Type == TransactionType.Split)
            .WithMessage("Splits are required for a split transaction.");

        RuleForEach(x => x.Splits!)
            .ChildRules(item =>
            {
                item.RuleFor(s => s.PersonName).NotEmpty().MaximumLength(200);
                item.RuleFor(s => s.PersonContact).MaximumLength(200).When(s => s.PersonContact is not null);
                item.RuleFor(s => s.Amount).GreaterThan(0);
            })
            .When(x => x.Type == TransactionType.Split && x.Splits is not null);

        RuleFor(x => x)
            .Must(x => x.Splits!.Sum(s => s.Amount) <= x.Amount)
            .When(x => x.Type == TransactionType.Split && x.Splits is { Count: > 0 })
            .WithMessage("The sum of split amounts must not exceed the transaction amount.");

        RuleFor(x => x.ToSourceId)
            .Must(c => c is { } id && id != Guid.Empty)
            .When(x => x.Type == TransactionType.Transfer)
            .WithMessage("ToSourceId is required for a transfer.");

        RuleFor(x => x)
            .Must(x => x.Type != TransactionType.Transfer || x.ToSourceId != x.SourceId)
            .When(x => x.Type == TransactionType.Transfer)
            .WithMessage("ToSourceId must differ from SourceId.");

        RuleFor(x => x.PersonName)
            .Must(n => !string.IsNullOrWhiteSpace(n))
            .When(x => x.Type is TransactionType.DebtBorrow or TransactionType.LoanGive)
            .WithMessage("PersonName is required for this transaction type.");

        RuleFor(x => x.DebtRecordId)
            .NotEmpty()
            .When(x => x.Type is TransactionType.DebtRepay or TransactionType.LoanCollect)
            .WithMessage("DebtRecordId is required for this transaction type.");

        RuleFor(x => x.DebtRecordId)
            .Must(c => !c.HasValue)
            .When(x => x.Type is not (TransactionType.DebtRepay or TransactionType.LoanCollect))
            .WithMessage("DebtRecordId is only used for debt repay and loan collect.");

        RuleFor(x => x.DueDate)
            .Must(d => !d.HasValue)
            .When(x => x.Type != TransactionType.DebtBorrow)
            .WithMessage("DueDate is only used for debt_borrow.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (!DebtTypes.Contains(cmd.Type)) return true;
                var src = await db.FinSources
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cmd.SourceId && s.SmoduleId == cmd.SmoduleId, ct)
                    .ConfigureAwait(false);
                return src is not null && !src.IsDeleted;
            })
            .WithMessage("Source was not found for this module or is inactive.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Transfer || cmd.ToSourceId is null)
                    return true;
                var to = await db.FinSources
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cmd.ToSourceId.Value && s.SmoduleId == cmd.SmoduleId, ct)
                    .ConfigureAwait(false);
                return to is not null && !to.IsDeleted;
            })
            .WithMessage("Destination source was not found for this module or is inactive.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Transfer || cmd.ToSourceId is null)
                    return true;
                var from = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                var to = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.ToSourceId.Value, ct).ConfigureAwait(false);
                if (from is null || to is null) return false;
                return from.SmoduleId == cmd.SmoduleId && to.SmoduleId == cmd.SmoduleId;
            })
            .WithMessage("Both sources must belong to the requested finance module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Transfer || cmd.ToSourceId is null)
                    return true;
                var from = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                var to = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.ToSourceId.Value, ct).ConfigureAwait(false);
                if (from is null || to is null) return false;
                return string.Equals(from.Currency, to.Currency, StringComparison.Ordinal);
            })
            .WithMessage("Both sources must use the same currency for a transfer.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type is not (TransactionType.Direct or TransactionType.Split))
                    return true;
                var cat = await db.FinCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cmd.CategoryId, ct).ConfigureAwait(false);
                return cat is not null && cat.SmoduleId == cmd.SmoduleId && cat.Kind == CategoryKind.Expense;
            })
            .WithMessage("Direct and split transactions require an expense category in this module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Income)
                    return true;
                var cat = await db.FinCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cmd.CategoryId, ct).ConfigureAwait(false);
                return cat is not null && cat.SmoduleId == cmd.SmoduleId && cat.Kind == CategoryKind.Income;
            })
            .WithMessage("Income transactions require an income category in this module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Deferred)
                    return true;
                var cat = await db.FinCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cmd.CategoryId, ct).ConfigureAwait(false);
                return cat is not null && cat.SmoduleId == cmd.SmoduleId && cat.Kind == CategoryKind.Expense;
            })
            .WithMessage("Deferred transactions require an expense category in this module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Deferred)
                    return true;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId && s.SmoduleId == cmd.SmoduleId, ct).ConfigureAwait(false);
                return src is not null && !src.IsDeleted && src.Type == SourceType.CreditCard;
            })
            .WithMessage("Deferred transactions require an active credit-card source in this module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.Deferred || cmd.BillingCycleId is not { } bcId)
                    return true;
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bcId, ct).ConfigureAwait(false);
                if (bc is null) return false;
                return bc.SourceId == cmd.SourceId && bc.SmoduleId == cmd.SmoduleId && bc.Status == BillingCycleStatus.Open;
            })
            .WithMessage("BillingCycleId must reference an open billing cycle for the same credit card and module.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.DebtRepay || cmd.DebtRecordId is not { } id)
                    return true;
                var debt = await db.FinDebtRecords.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct).ConfigureAwait(false);
                if (debt is null || debt.SmoduleId != cmd.SmoduleId || debt.IsDeleted)
                    return false;
                if (debt.Status != DebtStatus.Active || debt.Direction != DebtDirection.Borrowed)
                    return false;
                if (cmd.Amount > debt.RemainingAmount)
                    return false;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                return src is not null && string.Equals(src.Currency, debt.Currency, StringComparison.Ordinal);
            })
            .WithMessage("Invalid debt repay: record must be active borrowed debt in this module, amount must not exceed remaining, and source currency must match.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.LoanCollect || cmd.DebtRecordId is not { } id)
                    return true;
                var debt = await db.FinDebtRecords.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct).ConfigureAwait(false);
                if (debt is null || debt.SmoduleId != cmd.SmoduleId || debt.IsDeleted)
                    return false;
                if (debt.Status != DebtStatus.Active || debt.Direction != DebtDirection.Lent)
                    return false;
                if (cmd.Amount > debt.RemainingAmount)
                    return false;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                return src is not null && string.Equals(src.Currency, debt.Currency, StringComparison.Ordinal);
            })
            .WithMessage("Invalid loan collect: record must be active lent debt in this module, amount must not exceed remaining, and source currency must match.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type is not (TransactionType.Direct or TransactionType.Split))
                    return true;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId && s.SmoduleId == cmd.SmoduleId, ct).ConfigureAwait(false);
                return src is not null && src.Balance >= cmd.Amount;
            })
            .WithMessage("Insufficient balance on the selected source.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.LoanGive)
                    return true;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId && s.SmoduleId == cmd.SmoduleId, ct).ConfigureAwait(false);
                return src is not null && src.Balance >= cmd.Amount;
            })
            .WithMessage("Insufficient balance on the selected source.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (cmd.Type != TransactionType.DebtRepay)
                    return true;
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId && s.SmoduleId == cmd.SmoduleId, ct).ConfigureAwait(false);
                return src is not null && src.Balance >= cmd.Amount;
            })
            .WithMessage("Insufficient balance on the selected source.");
    }
}
