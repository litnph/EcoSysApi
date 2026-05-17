namespace PFP.Domain.Entities;

/// <summary>
/// Base class for entities that own a <c>*_HISTORY</c> companion table.
/// <para>
/// On every successful update, the <c>HistoryInterceptor</c> increments <see cref="Version"/>
/// and writes a snapshot row (with <c>changed_fields</c>) into the matching history table,
/// inside the same database transaction as the original write.
/// </para>
/// </summary>
public abstract class VersionedEntity : SoftDeletableEntity
{
    /// <summary>
    /// Monotonically increasing row version, starting at <c>1</c> for newly created records.
    /// Maintained by the history interceptor — handlers MUST NOT mutate it directly.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>Identifier of the <c>User</c> responsible for the most recent change.</summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>Identifier of the <c>UserSession</c> in which the most recent change was performed.</summary>
    public Guid? LastSessionId { get; set; }
}
