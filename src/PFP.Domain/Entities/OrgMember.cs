using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Membership of a <see cref="User"/> inside an <see cref="Organization"/>. Maps to <c>ORG_MEMBERS</c>.
/// <para>
/// A composite unique constraint on (<see cref="OrgId"/>, <see cref="UserId"/>) guarantees that a user
/// is mapped to a given organisation at most once. Removal is modelled as
/// <see cref="IsActive"/> = <c>false</c> + <see cref="LeftAt"/> set, never as a hard delete, so the
/// audit trail stays intact.
/// </para>
/// </summary>
public sealed class OrgMember : VersionedEntity
{
    /// <summary>FK to <see cref="Organization"/>.</summary>
    public Guid OrgId { get; set; }

    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Role inside the organisation; drives the org-level permission tier.</summary>
    public OrgRole Role { get; set; } = OrgRole.Member;

    /// <summary><c>false</c> after the user leaves or is removed; an inactive row is ignored by authorisation checks.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp at which the user accepted the invitation (or the row was created at registration).</summary>
    public DateTime? JoinedAt { get; set; }

    /// <summary>UTC timestamp at which the user lost access to the organisation; <c>null</c> while still active.</summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>FK to <see cref="User"/> — who issued the invitation. <c>null</c> for the owner of the personal org.</summary>
    public Guid? InvitedBy { get; set; }

    // ---- Navigation ----

    public Organization Org { get; set; } = null!;

    public User User { get; set; } = null!;

    /// <summary>Version history rows for this membership (append-only).</summary>
    public ICollection<OrgMemberHistory> History { get; set; } = new List<OrgMemberHistory>();
}
