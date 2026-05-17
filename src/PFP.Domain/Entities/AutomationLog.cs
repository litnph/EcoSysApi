using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>Append-only automation audit row. Maps to <c>AUTOMATION_LOGS</c>.</summary>
public sealed class AutomationLog : BaseEntity
{
    public Guid RuleId { get; set; }

    public DateTime TriggeredAt { get; set; }

    public RunStatus Status { get; set; }

    /// <summary>JSON describing actions attempted / completed.</summary>
    public string ActionsExecuted { get; set; } = "[]";

    public string? ErrorMessage { get; set; }

    /// <summary>Wall-clock duration of the run in milliseconds.</summary>
    public int DurationMs { get; set; }

    public AutomationRule Rule { get; set; } = null!;
}
