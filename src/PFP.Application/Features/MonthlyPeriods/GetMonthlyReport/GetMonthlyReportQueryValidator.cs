using FluentValidation;

namespace PFP.Application.Features.MonthlyPeriods.GetMonthlyReport;

/// <summary>Validates <see cref="GetMonthlyReportQuery"/>.</summary>
public sealed class GetMonthlyReportQueryValidator : AbstractValidator<GetMonthlyReportQuery>
{
    public GetMonthlyReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
    }
}
