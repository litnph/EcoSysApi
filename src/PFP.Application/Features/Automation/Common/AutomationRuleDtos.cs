using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.Common;

public sealed record AutomationRuleListItemDto(
    Guid Id,
    string Name,
    TriggerType TriggerType,
    bool IsActive,
    RunStatus? LastRunStatus,
    DateTime? LastRunAt);

public sealed record AutomationRuleDetailDto(
    Guid Id,
    Guid CreatedByUserId,
    string Name,
    TriggerType TriggerType,
    string TriggerValue,
    string Conditions,
    string Actions,
    bool IsActive,
    RunStatus? LastRunStatus,
    DateTime? LastRunAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record AutomationLogEntryDto(
    Guid Id,
    DateTime TriggeredAt,
    RunStatus Status,
    string ActionsExecuted,
    string? ErrorMessage,
    int DurationMs);

internal static class AutomationRuleDtoMapper
{
    public static AutomationRuleListItemDto ToListItem(AutomationRule r) =>
        new(r.Id, r.Name, r.TriggerType, r.IsActive, r.LastRunStatus, r.LastRunAt);

    public static AutomationRuleDetailDto ToDetail(AutomationRule r) =>
        new(
            r.Id,
            r.CreatedByUserId,
            r.Name,
            r.TriggerType,
            r.TriggerValue,
            r.Conditions,
            r.Actions,
            r.IsActive,
            r.LastRunStatus,
            r.LastRunAt,
            r.CreatedAt,
            r.UpdatedAt);
}
