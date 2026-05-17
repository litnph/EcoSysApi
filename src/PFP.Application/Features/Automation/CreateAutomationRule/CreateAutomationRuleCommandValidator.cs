using FluentValidation;
using PFP.Application.Features.Automation.Common;

namespace PFP.Application.Features.Automation.CreateAutomationRule;

public sealed class CreateAutomationRuleCommandValidator : AbstractValidator<CreateAutomationRuleCommand>
{
    public CreateAutomationRuleCommandValidator()
    {
        RuleFor(x => x.SmoduleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerType).IsInEnum();
        RuleFor(x => x.TriggerValue).NotEmpty().MaximumLength(2048);

        RuleFor(x => x.Conditions).Custom((json, ctx) =>
        {
            if (!AutomationPayloadValidator.IsJsonArray(json, out var err))
                ctx.AddFailure(nameof(CreateAutomationRuleCommand.Conditions), err ?? "Invalid JSON array.");
        });

        RuleFor(x => x.Actions).Custom((json, ctx) =>
        {
            if (!AutomationPayloadValidator.TryValidateActions(json, out var err))
                ctx.AddFailure(nameof(CreateAutomationRuleCommand.Actions), err ?? "Invalid actions.");
        });
    }
}
