using FluentValidation;
using PFP.Application.Features.Automation.Common;

namespace PFP.Application.Features.Automation.UpdateAutomationRule;

public sealed class UpdateAutomationRuleCommandValidator : AbstractValidator<UpdateAutomationRuleCommand>
{
    public UpdateAutomationRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TriggerType).IsInEnum();
        RuleFor(x => x.TriggerValue).NotEmpty().MaximumLength(2048);

        RuleFor(x => x.Conditions).Custom((json, ctx) =>
        {
            if (!AutomationPayloadValidator.IsJsonArray(json, out var err))
                ctx.AddFailure(nameof(UpdateAutomationRuleCommand.Conditions), err ?? "Invalid JSON array.");
        });

        RuleFor(x => x.Actions).Custom((json, ctx) =>
        {
            if (!AutomationPayloadValidator.TryValidateActions(json, out var err))
                ctx.AddFailure(nameof(UpdateAutomationRuleCommand.Actions), err ?? "Invalid actions.");
        });
    }
}
