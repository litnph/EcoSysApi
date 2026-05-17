using FluentValidation;

namespace PFP.Application.Features.Automation.ToggleAutomationRule;

public sealed class ToggleAutomationRuleCommandValidator : AbstractValidator<ToggleAutomationRuleCommand>
{
    public ToggleAutomationRuleCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
