namespace PFP.Domain.Entities;

/// <summary>
/// Base class for entities that participate in soft-delete semantics.
/// <para>
/// Rows are NEVER hard-deleted. The <c>SoftDeleteInterceptor</c> in the Infrastructure layer
/// rewrites <c>EntityState.Deleted</c> into <c>Modified</c> and sets
/// <see cref="IsDeleted"/>, <see cref="DeletedAt"/>, and <see cref="DeletedBy"/>.
/// </para>
/// <para>
/// A global query filter is registered for every <see cref="SoftDeletableEntity"/>, so soft-deleted
/// rows are invisible to ordinary queries. Use <c>IgnoreQueryFilters()</c> only in admin/audit paths.
/// </para>
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity
{
    /// <summary>True if this row has been soft-deleted. Toggled by the soft-delete interceptor.</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>UTC timestamp of the soft-delete, or <c>null</c> while the row is alive.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Identifier of the <c>User</c> that requested the deletion.</summary>
    public Guid? DeletedBy { get; set; }
}
