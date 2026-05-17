namespace PFP.Domain.Entities;

/// <summary>
/// Per-entity-type retention policy for <see cref="AuditLog"/> rows.
/// Maps to <c>AUDIT_LOG_RETENTIONS</c>.
/// <para>
/// Consumed weekly by the <c>AuditLogRetention</c> Hangfire job: rows older than
/// <see cref="RetainDays"/> are first archived to cold storage (when
/// <see cref="ArchiveBeforeDelete"/> is set) and then physically removed from <c>AUDIT_LOGS</c>.
/// </para>
/// <para>
/// A <see cref="EntityType"/> of <c>null</c> represents the catch-all default policy applied when
/// no entity-specific row matches.
/// </para>
/// </summary>
public sealed class AuditLogRetention : BaseEntity
{
    /// <summary>
    /// CLR type name the policy applies to (e.g. <c>FinTransaction</c>);
    /// <c>null</c> for the global default.
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>Maximum age, in days, of audit rows to keep online.</summary>
    public int RetainDays { get; set; }

    /// <summary>
    /// When <c>true</c>, the worker uploads expiring rows to <see cref="ArchiveStorageKeyPrefix"/>
    /// (Cloudflare R2) before deletion. When <c>false</c>, rows are deleted in place.
    /// </summary>
    public bool ArchiveBeforeDelete { get; set; } = true;

    /// <summary>R2 key prefix into which archive batches are written.</summary>
    public string? ArchiveStorageKeyPrefix { get; set; }

    /// <summary>Soft-disable flag — an inactive policy is ignored by the retention job.</summary>
    public bool IsActive { get; set; } = true;
}
