using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;

/// <summary>FluentValidation rules for <see cref="PayBillingCycleCommand"/>.</summary>
public sealed class PayBillingCycleCommandValidator : AbstractValidator<PayBillingCycleCommand>
{
    /// <summary>Registers validation rules.</summary>
    public PayBillingCycleCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.CycleId).NotEmpty();
        RuleFor(x => x.PaymentSourceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                return bc is not null && (bc.Status == BillingCycleStatus.Closed || bc.Status == BillingCycleStatus.Overdue);
            })
            .WithMessage("Billing cycle must be closed or overdue to accept a payment.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                if (bc is null) return true;
                return cmd.PaymentSourceId != bc.SourceId;
            })
            .WithMessage("Payment source cannot be the same as the credit card source for this cycle.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var paySrc = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.PaymentSourceId, ct).ConfigureAwait(false);
                return paySrc is not null && !paySrc.IsDeleted;
            })
            .WithMessage("Payment source was not found or is inactive.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                var paySrc = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.PaymentSourceId, ct).ConfigureAwait(false);
                if (bc is null || paySrc is null) return false;
                return paySrc is not null;
            })
            .WithMessage("Payment source must belong to the same finance module as the billing cycle.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                var paySrc = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.PaymentSourceId, ct).ConfigureAwait(false);
                if (bc is null || paySrc is null) return false;
                var card = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == bc.SourceId, ct).ConfigureAwait(false);
                if (card is null) return false;
                return string.Equals(paySrc.Currency, card.Currency, StringComparison.Ordinal);
            })
            .WithMessage("Payment source currency must match the credit card currency.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                if (bc is null) return false;
                var remaining = bc.TotalAmount - bc.PaidAmount;
                return cmd.Amount <= remaining;
            })
            .WithMessage("Amount cannot exceed the remaining balance for this cycle (TotalAmount - PaidAmount).");
    }
}
