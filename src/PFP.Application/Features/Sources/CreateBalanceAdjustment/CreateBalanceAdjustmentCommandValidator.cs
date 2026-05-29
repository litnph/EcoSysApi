using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Sources.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Sources.CreateBalanceAdjustment;

public sealed class CreateBalanceAdjustmentCommandValidator : AbstractValidator<CreateBalanceAdjustmentCommand>
{
    public CreateBalanceAdjustmentCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Note).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Amount).NotEqual(0);

        RuleFor(x => x.TxnDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1));

        RuleFor(x => x.SourceId).MustAsync(async (id, ct) =>
        {
            var source = await db.FinSources.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct)
                .ConfigureAwait(false);
            return source is not null
                   && !source.IsDeleted
                   && AssetSourceRules.SupportsBalanceLedger(source.Type);
        }).WithMessage("Source must exist and support balance adjustments (not a credit card).");
    }
}
