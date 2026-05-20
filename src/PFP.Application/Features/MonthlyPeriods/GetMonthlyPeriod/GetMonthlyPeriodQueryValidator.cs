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
RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
}
}
