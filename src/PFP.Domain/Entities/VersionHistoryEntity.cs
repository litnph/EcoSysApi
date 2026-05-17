using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Shared schema for every <c>*_HISTORY</c> table (spec §3.7).
/// <para>
/// One row is appended on every successful create / update / soft-delete of a
/// <see cref="VersionedEntity"/>-derived row, by the <c>HistoryInterceptor</c> in the same DB
/// transaction as the originating write. History rows are append-only — they never carry
/// soft-delete columns of their own, hence <see cref="BaseEntity"/> (not <see cref="SoftDeletableEntity"/>).
/// </para>
/// <para>
/// The concrete subclass adds a strongly-typed <c>EntityId</c> FK and a navigation property to its
/// parent entity. Each subclass is configured with a composite index
/// (<c>EntityId</c>, <see cref="Version"/>) per spec §3.8.
/// </para>
/// </summary>
public abstract class VersionHistoryEntity : BaseEntity
{
    /// <summary>Row version captured by this history row; matches <see cref="VersionedEntity.Version"/> after the change.</summary>
    public int Version { get; set; }

    /// <summary>FK to <see cref="User"/> — who performed the change. <c>null</c> for system-driven writes.</summary>
    public Guid? ChangedBy { get; set; }

    /// <summary>FK to <see cref="UserSession"/> — the session in which the change happened. <c>null</c> outside an HTTP context.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>Kind of change (<see cref="HistoryChangeType.Created"/> / <see cref="HistoryChangeType.Updated"/> / <see cref="HistoryChangeType.Deleted"/> / <see cref="HistoryChangeType.Restored"/>).</summary>
    public HistoryChangeType ChangeType { get; set; }

    /// <summary>JSON array of field names that changed in this revision (only populated for <see cref="HistoryChangeType.Updated"/>).</summary>
    public string? ChangedFields { get; set; }

    /// <summary>JSON snapshot of the row AFTER the change. Stored as <c>jsonb</c> in PostgreSQL.</summary>
    public string? Snapshot { get; set; }

    /// <summary>Optional human-readable reason for the change (recorded by handlers that ask the user "why").</summary>
    public string? ChangeReason { get; set; }
}
