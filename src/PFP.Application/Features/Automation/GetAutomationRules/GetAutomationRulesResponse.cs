using PFP.Application.Features.Automation.Common;

namespace PFP.Application.Features.Automation.GetAutomationRules;

public sealed record GetAutomationRulesResponse(IReadOnlyList<AutomationRuleListItemDto> Items);
