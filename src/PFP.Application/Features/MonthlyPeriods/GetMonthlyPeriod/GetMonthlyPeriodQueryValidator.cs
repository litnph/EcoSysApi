using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriod;

/// <summary>FluentValidation rules for <see cref="GetMonthlyPeriodQuery"/>.</summary>
public sealed class GetMonthlyPeriodQueryValidator : AbstractValidator<GetMonthlyPeriodQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetMonthlyPeriodQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.SpaceModules.AnyAsync(m => m.Id == q.SmoduleId && m.ModuleCode == ModuleCode.Finance, ct)
                    .ConfigureAwait(false))
            .WithMessage("Finance module was not found for this id.");
    }
}
