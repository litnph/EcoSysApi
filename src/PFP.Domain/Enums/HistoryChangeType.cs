namespace PFP.Domain.Enums;

/// <summary>
/// Type of change recorded in a <c>*_HISTORY</c> row (per spec §3.3 / §3.7).
/// <para>
/// Distinct from <see cref="AuditAction"/>: history captures the version evolution of one entity row
/// (with a strictly increasing <c>Version</c>), while audit captures every write across the database.
/// History is written by the <c>HistoryInterceptor</c> for entities deriving from <c>VersionedEntity</c>.
/// </para>
/// </summary>
public enum HistoryChangeType
{
    /// <summary><c>created</c> — first version (<c>Version = 1</c>) of the entity row.</summary>
    Created = 1,

    /// <summary><c>updated</c> — a subsequent revision; <c>changed_fields</c> lists what moved.</summary>
    Updated = 2,

    /// <summary><c>deleted</c> — soft-delete revision (the entity row has <c>is_deleted = true</c>).</summary>
    Deleted = 3,

    /// <summary><c>restored</c> — soft-deleted row was restored by an admin / data-recovery flow.</summary>
    Restored = 4,

    /// <summary><c>cancelled</c> — installment plan was cancelled (domain-specific).</summary>
    Cancelled = 5,
}
