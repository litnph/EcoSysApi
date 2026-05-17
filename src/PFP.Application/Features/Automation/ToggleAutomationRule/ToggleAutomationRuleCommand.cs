using MediatR;

namespace PFP.Application.Features.Automation.ToggleAutomationRule;

public sealed record ToggleAutomationRuleCommand(Guid Id) : IRequest<ToggleAutomationRuleResponse>;
