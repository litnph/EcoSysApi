namespace PFP.Domain.Enums;

/// <summary>
/// Type of write operation captured in an <c>AUDIT_LOGS</c> row.
/// <para>
/// The <c>AuditInterceptor</c> derives this value from <c>EntityState</c>:
/// <c>Added</c> → <see cref="Created"/>, <c>Modified</c> → <see cref="Updated"/>,
/// soft-delete (rewritten from <c>Deleted</c>) → <see cref="Deleted"/>.
/// </para>
/// </summary>
public enum AuditAction
{
    /// <summary><c>created</c> — a new row was inserted; <c>before_snapshot</c> is <c>null</c>.</summary>
    Created = 1,

    /// <summary><c>updated</c> — an existing row was modified; both snapshots are populated.</summary>
    Updated = 2,

    /// <summary><c>deleted</c> — the row was soft-deleted; <c>after_snapshot</c> reflects the deletion-marked state.</summary>
    Deleted = 3,
}
