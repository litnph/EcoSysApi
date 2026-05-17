using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;

/// <summary>FluentValidation rules for <see cref="CloseBillingCycleCommand"/>.</summary>
public sealed class CloseBillingCycleCommandValidator : AbstractValidator<CloseBillingCycleCommand>
{
    /// <summary>Registers validation rules.</summary>
    public CloseBillingCycleCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.CycleId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                return bc is not null;
            })
            .WithMessage("Billing cycle was not found.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var bc = await db.FinBillingCycles.AsNoTracking().FirstOrDefaultAsync(b => b.Id == cmd.CycleId, ct).ConfigureAwait(false);
                return bc is null || bc.Status == BillingCycleStatus.Open;
            })
            .WithMessage("Only an open billing cycle can be closed.");
    }
}
