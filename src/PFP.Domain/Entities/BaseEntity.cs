namespace PFP.Domain.Entities;

/// <summary>
/// Base class for every user-managed entity in the platform.
/// Provides the universal identity and timestamp surface.
/// <para>
/// <c>CreatedAt</c> and <c>UpdatedAt</c> are populated by EF Core interceptors
/// (see <c>AuditInterceptor</c> in the Infrastructure layer) — handlers MUST NOT set them manually.
/// </para>
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Globally unique identifier. Defaults to <see cref="Guid.NewGuid"/> so that aggregates
    /// can be referenced by <c>Id</c> before they reach the database.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>UTC timestamp at which the row was first persisted. Set once by the audit interceptor.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last <c>SaveChanges</c> that touched this row. Maintained by the audit interceptor.</summary>
    public DateTime UpdatedAt { get; set; }
}
