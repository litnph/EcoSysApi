using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>User-defined automation for a finance module. Maps to <c>AUTOMATION_RULES</c>.</summary>
public sealed class AutomationRule : SoftDeletableEntity
{
    /// <summary>
    /// User who owns this rule’s side-effects (MediatR handlers impersonate this identity during execution).
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public TriggerType TriggerType { get; set; }

    /// <summary>Trigger-specific payload (date string, threshold JSON, etc.).</summary>
    public string TriggerValue { get; set; } = string.Empty;

    /// <summary>JSON array of condition objects (<c>field</c>, <c>op</c>, <c>value</c>).</summary>
    public string Conditions { get; set; } = "[]";

    /// <summary>JSON array of action objects (<c>type</c> + payload).</summary>
    public string Actions { get; set; } = "[]";

    public bool IsActive { get; set; } = true;

    public DateTime? LastRunAt { get; set; }

    /// <summary>Result of the last job evaluation / execution.</summary>
    public RunStatus? LastRunStatus { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<AutomationLog> Logs { get; set; } = new List<AutomationLog>();
}
