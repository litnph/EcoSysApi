using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Append-only audit row recording every write performed by a user (or by a CQRS handler on behalf of a user).
/// Maps to <c>AUDIT_LOGS</c>.
/// <para>
/// Produced automatically by the <c>AuditInterceptor</c> in the Infrastructure layer, in the same database
/// transaction as the originating <c>SaveChanges</c>. Handlers MUST NOT insert audit rows directly.
/// </para>
/// <para>
/// Per the spec the table carries two composite indexes:
/// (<see cref="EntityType"/>, <see cref="EntityId"/>, <see cref="BaseEntity.CreatedAt"/>) and
/// (<see cref="UserId"/>, <see cref="BaseEntity.CreatedAt"/>) — to be configured in the EF mapping.
/// </para>
/// </summary>
public sealed class AuditLog : BaseEntity
{
    /// <summary>FK to <see cref="User"/>; <c>null</c> when the change was performed by a background job
    /// (those go to <see cref="SystemEventLog"/> instead, but a few flows write to both).</summary>
    public Guid? UserId { get; set; }

    /// <summary>FK to <see cref="UserSession"/> (when known) — lets investigators trace a write back to a device.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>CLR type name of the affected entity (e.g. <c>FinTransaction</c>).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Identifier of the affected entity row.</summary>
    public Guid EntityId { get; set; }

    /// <summary>What kind of write produced this row.</summary>
    public AuditAction Action { get; set; }

    /// <summary>JSON snapshot of the row before the change (<c>null</c> for <see cref="AuditAction.Created"/>).</summary>
    public string? BeforeSnapshot { get; set; }

    /// <summary>JSON snapshot of the row after the change.</summary>
    public string? AfterSnapshot { get; set; }

    /// <summary>JSON array of field names that actually changed (only set for <see cref="AuditAction.Updated"/>).</summary>
    public string? ChangedFields { get; set; }

    /// <summary>IP address of the originating HTTP request, when available.</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent header of the originating HTTP request, when available.</summary>
    public string? UserAgent { get; set; }

    // ---- Navigation ----

    public User? User { get; set; }
}
