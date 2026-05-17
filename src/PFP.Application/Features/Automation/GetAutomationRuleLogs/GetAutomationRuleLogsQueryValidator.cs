using FluentValidation;

namespace PFP.Application.Features.Automation.GetAutomationRuleLogs;

public sealed class GetAutomationRuleLogsQueryValidator : AbstractValidator<GetAutomationRuleLogsQuery>
{
    public GetAutomationRuleLogsQueryValidator()
    {
        RuleFor(x => x.RuleId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
