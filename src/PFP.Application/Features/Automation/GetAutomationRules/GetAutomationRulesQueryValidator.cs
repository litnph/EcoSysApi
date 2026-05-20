using FluentValidation;

namespace PFP.Application.Features.Automation.GetAutomationRules;

public sealed class GetAutomationRulesQueryValidator : AbstractValidator<GetAutomationRulesQuery>
{
    public GetAutomationRulesQueryValidator()
    {
    }
}
