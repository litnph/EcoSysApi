using MediatR;

namespace PFP.Application.Features.Automation.DeleteAutomationRule;

public sealed record DeleteAutomationRuleCommand(Guid Id) : IRequest<DeleteAutomationRuleResponse>;
