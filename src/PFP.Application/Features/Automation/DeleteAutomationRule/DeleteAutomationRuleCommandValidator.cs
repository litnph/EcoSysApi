using FluentValidation;

namespace PFP.Application.Features.Automation.DeleteAutomationRule;

public sealed class DeleteAutomationRuleCommandValidator : AbstractValidator<DeleteAutomationRuleCommand>
{
    public DeleteAutomationRuleCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
