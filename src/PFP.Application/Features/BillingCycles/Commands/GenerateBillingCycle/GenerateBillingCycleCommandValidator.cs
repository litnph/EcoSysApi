using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;

/// <summary>FluentValidation rules for <see cref="GenerateBillingCycleCommand"/>.</summary>
public sealed class GenerateBillingCycleCommandValidator : AbstractValidator<GenerateBillingCycleCommand>
{
    /// <summary>Registers validation rules.</summary>
    public GenerateBillingCycleCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SourceId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                return src is not null && !src.IsDeleted && src.Type == SourceType.CreditCard;
            })
            .WithMessage("Source must exist, be active, and be a credit card.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var src = await db.FinSources.AsNoTracking().FirstOrDefaultAsync(s => s.Id == cmd.SourceId, ct).ConfigureAwait(false);
                return src is not null
                       && src.StatementDay is { } sd && sd is >= 1 and <= 31
                       && src.PaymentDueDay is { } pdd && pdd >= 1;
            })
            .WithMessage("Credit card must have StatementDay and PaymentDueDay configured.");

        RuleFor(x => x)
            .Must(cmd =>
                (cmd.StatementYear is null && cmd.StatementMonth is null)
                || (cmd.StatementYear is not null && cmd.StatementMonth is not null))
            .WithMessage("Statement year and month must both be provided or both omitted.");

        RuleFor(x => x.StatementMonth)
            .InclusiveBetween(1, 12)
            .When(x => x.StatementMonth is not null);

        RuleFor(x => x.StatementYear)
            .InclusiveBetween(2000, 2100)
            .When(x => x.StatementYear is not null);
    }
}
