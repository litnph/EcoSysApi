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
}
}
