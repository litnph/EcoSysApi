using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Validates <see cref="GetMonthlyReportQuery"/>.</summary>
public sealed class GetMonthlyReportQueryValidator : AbstractValidator<GetMonthlyReportQuery>
{
    public GetMonthlyReportQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);

        RuleFor(x => x)
            .Must(q =>
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var target = new DateOnly(q.Year, q.Month, 1);
                return target <= today;
            })
            .WithMessage("Cannot load a report for a future month.");

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.SpaceModules.AnyAsync(m => m.Id == q.SmoduleId && m.ModuleCode == ModuleCode.Finance, ct)
                    .ConfigureAwait(false))
            .WithMessage("Finance module was not found for this id.");
    }
}
