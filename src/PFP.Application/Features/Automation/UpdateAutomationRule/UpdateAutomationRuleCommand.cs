using MediatR;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.UpdateAutomationRule;

public sealed record UpdateAutomationRuleCommand(
    Guid Id,
    string Name,
    TriggerType TriggerType,
    string TriggerValue,
    string Conditions,
    string Actions,
    bool IsActive) : IRequest<UpdateAutomationRuleResponse>;
