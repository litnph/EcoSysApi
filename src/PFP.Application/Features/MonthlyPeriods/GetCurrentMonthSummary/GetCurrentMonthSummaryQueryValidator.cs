using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.GetCurrentMonthSummary;

/// <summary>FluentValidation rules for <see cref="GetCurrentMonthSummaryQuery"/>.</summary>
public sealed class GetCurrentMonthSummaryQueryValidator : AbstractValidator<GetCurrentMonthSummaryQuery>
{
    /// <summary>Registers validation rules.</summary>
    public GetCurrentMonthSummaryQueryValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.SmoduleId).NotEmpty();

        RuleFor(x => x)
            .MustAsync(async (q, ct) =>
                await db.SpaceModules.AnyAsync(m => m.Id == q.SmoduleId && m.ModuleCode == ModuleCode.Finance, ct)
                    .ConfigureAwait(false))
            .WithMessage("Finance module was not found for this id.");
    }
}
