using FluentValidation;
using PFP.Application.Common;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Splits.SettleSplit;

/// <summary>Validates <see cref="SettleSplitCommand"/>.</summary>
public sealed class SettleSplitCommandValidator : AbstractValidator<SettleSplitCommand>
{
    public SettleSplitCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SplitId).NotEmpty();
        RuleFor(x => x.PaymentSourceId).NotEmpty();

        RuleFor(x => x.Amount)
            .Must(a => !a.HasValue || a.Value > 0)
            .WithMessage("Amount, when provided, must be greater than zero.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var split = await db.FinTxnSplits
                    .AsNoTracking()
                    .Include(s => s.Transaction)
                    .FirstOrDefaultAsync(s => s.Id == cmd.SplitId, ct)
                    .ConfigureAwait(false);
                if (split is null)
                    return false;
                return split.Status == SplitStatus.Pending;
            })
            .WithMessage("Split was not found or is not pending.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                if (!cmd.Amount.HasValue)
                    return true;
                var split = await db.FinTxnSplits
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cmd.SplitId, ct)
                    .ConfigureAwait(false);
                return split is not null
                    && cmd.Amount is { } amt
                    && CurrencyUnits.FromWhole(amt) <= split.Amount;
            })
            .WithMessage("Amount cannot exceed the split total.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var split = await db.FinTxnSplits
                    .AsNoTracking()
                    .Include(s => s.Transaction)
                    .FirstOrDefaultAsync(s => s.Id == cmd.SplitId, ct)
                    .ConfigureAwait(false);
                if (split is null)
                    return false;
                var src = await db.FinSources
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == cmd.PaymentSourceId, ct)
                    .ConfigureAwait(false);
                return src is not null
                       && !src.IsDeleted
                       && true
                       && string.Equals(src.Currency, split.Transaction.Currency, StringComparison.Ordinal);
            })
            .WithMessage("Payment source was not found, is inactive, or does not match the split module/currency.");
    }
}
