using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.CreateAutomationRule;

public sealed record CreateAutomationRuleCommand(
    string Name,
    TriggerType TriggerType,
    string TriggerValue,
    string Conditions,
    string Actions) : IRequest<CreateAutomationRuleResponse>;
