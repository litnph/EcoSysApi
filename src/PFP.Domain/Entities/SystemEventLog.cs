namespace PFP.Domain.Entities;

/// <summary>
/// Append-only log of system-initiated work — Hangfire jobs, schedulers, infra reconciliations.
/// Maps to <c>SYSTEM_EVENT_LOGS</c>.
/// <para>
/// Complements <see cref="AuditLog"/>, which captures user-attributed writes. System events that
/// also touch user-owned rows will produce both a <see cref="SystemEventLog"/> entry (for the job
/// run) and one or more <see cref="AuditLog"/> entries (for each affected row).
/// </para>
/// </summary>
public sealed class SystemEventLog : BaseEntity
{
    /// <summary>
    /// Snake_case identifier of the event,
    /// e.g. <c>billing_cycles.generated</c>, <c>monthly_period.closed</c>, <c>data_export.processed</c>.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Optional CLR type name when the event targets a specific entity.</summary>
    public string? EntityType { get; set; }

    /// <summary>Optional identifier of the affected row.</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Hangfire job name when the event was emitted from a background job.</summary>
    public string? JobName { get; set; }

    /// <summary>Hangfire job identifier (when applicable) — useful for correlating with the dashboard.</summary>
    public string? JobId { get; set; }

    /// <summary>Free-form JSON payload — inputs and aggregated results of the run.</summary>
    public string? Payload { get; set; }

    /// <summary>Outcome marker: <c>success</c> | <c>failure</c> | <c>skipped</c>.</summary>
    public string Status { get; set; } = "success";

    /// <summary>Error message captured when <see cref="Status"/> is <c>failure</c>.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Stack trace captured when <see cref="Status"/> is <c>failure</c>.</summary>
    public string? StackTrace { get; set; }

    /// <summary>Wall-clock duration of the run in milliseconds.</summary>
    public long? DurationMs { get; set; }
}
