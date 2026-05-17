using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.DeleteTransaction;

/// <summary>FluentValidation rules for <see cref="DeleteTransactionCommand"/>.</summary>
public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    /// <summary>Registers validation rules.</summary>
    public DeleteTransactionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.TransactionId).NotEmpty();

        RuleFor(x => x.Reason).MaximumLength(2000).When(x => x.Reason is not null);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                return txn is not null;
            })
            .WithMessage("Transaction was not found.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                return txn is null || !txn.IsDeleted;
            })
            .WithMessage("Transaction is already deleted.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                return txn is null || txn.Type != TransactionType.Reversal;
            })
            .WithMessage("Reversal transactions cannot be deleted.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                if (txn is null) return true;
                return txn.Type is TransactionType.Direct or TransactionType.Income or TransactionType.Transfer or TransactionType.Deferred;
            })
            .WithMessage("This transaction type cannot be deleted through this command.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                if (txn?.BillingCycleId is not { } bcId) return true;
                var cycle = await db.FinBillingCycles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == bcId, ct)
                    .ConfigureAwait(false);
                if (cycle is null) return false;
                return cycle.Status == BillingCycleStatus.Open;
            })
            .WithMessage("Không thể xóa giao dịch trong kỳ sao kê đã đóng");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var txn = await db.FinTransactions
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == cmd.TransactionId, ct)
                    .ConfigureAwait(false);
                if (txn?.MonthlyPeriodId is not { } mpId) return true;
                var period = await db.FinMonthlyPeriods
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == mpId, ct)
                    .ConfigureAwait(false);
                if (period is null) return false;
                return period.Status == PeriodStatus.Open;
            })
            .WithMessage("Không thể xóa giao dịch trong kỳ tháng đã đóng.");
    }
}
