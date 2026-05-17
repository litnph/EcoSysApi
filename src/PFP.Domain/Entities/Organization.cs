namespace PFP.Domain.Entities;

/// <summary>
/// Top-level multi-tenant container. Maps to <c>ORGANIZATIONS</c>.
/// <para>
/// On registration the bootstrap flow auto-creates one personal organization per user
/// (<see cref="IsPersonal"/> = <c>true</c>, name = <c>"{user.FullName}'s Personal"</c>) — see spec §4.1.
/// Non-personal organizations (family, business, …) are created later through the
/// <c>POST /organizations</c> endpoint.
/// </para>
/// <para>
/// <see cref="Slug"/> carries a unique constraint and is the URL-safe identifier used in front-end routes.
/// <see cref="DefaultCurrency"/> is an ISO 4217 code (default <c>VND</c>) and is inherited by every
/// finance source created inside the organisation unless explicitly overridden.
/// </para>
/// </summary>
public sealed class Organization : VersionedEntity
{
    /// <summary>
    /// <c>true</c> for the auto-provisioned personal organisation owned by exactly one user;
    /// such organisations cannot be invited into and cannot have their owner transferred.
    /// </summary>
    public bool IsPersonal { get; set; }

    /// <summary>URL-safe identifier (lower-case, kebab-case). Globally unique.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Display name shown in the org switcher and across the UI.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>FK to <see cref="User"/>. Each organisation has exactly one owner at a time;
    /// transfer is performed by an explicit handler that updates this field and the matching
    /// <see cref="OrgMember"/> roles in a single transaction.</summary>
    public Guid OwnerId { get; set; }

    /// <summary>ISO 4217 currency code used as the default for new finance sources. Default: <c>VND</c>.</summary>
    public string DefaultCurrency { get; set; } = "VND";

    /// <summary>Optional free-form description shown on the organisation profile page.</summary>
    public string? Description { get; set; }

    // ---- Navigation ----

    public User Owner { get; set; } = null!;

    /// <summary>All members of the organisation (including the owner).</summary>
    public ICollection<OrgMember> Members { get; set; } = new List<OrgMember>();

    /// <summary>All spaces (root and nested) belonging to this organisation.</summary>
    public ICollection<Space> Spaces { get; set; } = new List<Space>();

    /// <summary>Version history rows for this organisation (append-only).</summary>
    public ICollection<OrganizationHistory> History { get; set; } = new List<OrganizationHistory>();
}
