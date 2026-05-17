using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Membership of a <see cref="User"/> inside a <see cref="Space"/>. Maps to <c>SPACE_MEMBERS</c>.
/// <para>
/// Per spec §4.5, when a child space is created the platform cascades a <see cref="SpaceMember"/> row
/// for every member of the parent space, with <see cref="Inherited"/> = <c>true</c> and
/// <see cref="InheritedFromSpaceId"/> pointing at the ancestor that supplied the membership.
/// Storing the materialised inherited rows means authorisation checks never have to walk the
/// space tree at request time.
/// </para>
/// <para>
/// A composite unique constraint on (<see cref="SpaceId"/>, <see cref="UserId"/>) prevents duplicates;
/// when a user is granted a stronger role directly on a child space, the inherited row is overwritten in place.
/// </para>
/// </summary>
public sealed class SpaceMember : VersionedEntity
{
    /// <summary>FK to <see cref="Space"/>.</summary>
    public Guid SpaceId { get; set; }

    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Role inside the space; drives the space-level permission tier.</summary>
    public SpaceRole Role { get; set; } = SpaceRole.Viewer;

    /// <summary>
    /// <c>true</c> when this membership was created as a side-effect of an ancestor membership;
    /// <c>false</c> when the user was added explicitly to this space.
    /// </summary>
    public bool Inherited { get; set; }

    /// <summary>FK to the ancestor <see cref="Space"/> from which this membership was cascaded;
    /// <c>null</c> for direct memberships.</summary>
    public Guid? InheritedFromSpaceId { get; set; }

    /// <summary>FK to <see cref="User"/> — who granted access to this space. <c>null</c> for inherited rows.</summary>
    public Guid? InvitedBy { get; set; }

    /// <summary>UTC timestamp at which the user accepted the invitation (or was cascaded in).</summary>
    public DateTime? JoinedAt { get; set; }

    /// <summary>UTC timestamp at which the user lost access; <c>null</c> while still a member.</summary>
    public DateTime? LeftAt { get; set; }

    // ---- Navigation ----

    public Space Space { get; set; } = null!;

    public User User { get; set; } = null!;

    public Space? InheritedFromSpace { get; set; }

    /// <summary>Version history rows for this membership (append-only).</summary>
    public ICollection<SpaceMemberHistory> History { get; set; } = new List<SpaceMemberHistory>();
}
