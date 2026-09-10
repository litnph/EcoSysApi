using FluentValidation;

namespace PFP.Application.Features.Budgets.GetCategoryBudgets;

public sealed class GetCategoryBudgetsQueryValidator : AbstractValidator<GetCategoryBudgetsQuery>
{
    public GetCategoryBudgetsQueryValidator()
    {
        RuleFor(query => query.Year)
            .InclusiveBetween(1900, 2200)
            .When(query => query.Year.HasValue);
        RuleFor(query => query.Month)
            .InclusiveBetween(1, 12)
            .When(query => query.Month.HasValue);
        RuleFor(query => query)
            .Must(query => query.Year.HasValue == query.Month.HasValue)
            .WithMessage("Year and month must both be provided or both omitted.");
    }
}
