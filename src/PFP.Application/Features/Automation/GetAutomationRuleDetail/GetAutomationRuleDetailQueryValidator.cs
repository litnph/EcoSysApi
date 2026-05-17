using FluentValidation;

namespace PFP.Application.Features.Automation.GetAutomationRuleDetail;

public sealed class GetAutomationRuleDetailQueryValidator : AbstractValidator<GetAutomationRuleDetailQuery>
{
    public GetAutomationRuleDetailQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}
