using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyPeriodsList;

/// <summary>Validates <see cref="GetMonthlyPeriodsListQuery"/>.</summary>
public sealed class GetMonthlyPeriodsListQueryValidator : AbstractValidator<GetMonthlyPeriodsListQuery>
{
    public GetMonthlyPeriodsListQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.SpaceModules.AnyAsync(m => m.Id == q.SmoduleId && m.ModuleCode == ModuleCode.Finance, ct)
                    .ConfigureAwait(false))
            .WithMessage("Finance module was not found for this id.");
    }
}
