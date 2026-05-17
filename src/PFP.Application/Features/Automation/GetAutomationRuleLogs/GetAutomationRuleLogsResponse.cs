using PFP.Application.Features.Automation.Common;

namespace PFP.Application.Features.Automation.GetAutomationRuleLogs;

public sealed record GetAutomationRuleLogsResponse(
    IReadOnlyList<AutomationLogEntryDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
