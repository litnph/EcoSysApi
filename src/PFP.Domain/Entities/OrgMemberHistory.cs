namespace PFP.Domain.Entities;

/// <summary>
/// Version-history row for <see cref="OrgMember"/>. Maps to <c>ORG_MEMBERS_HISTORY</c> (spec §3.7).
/// <para>
/// Records role promotions / demotions and the join → active → left transitions so that an
/// organisation's membership history can be replayed even after a user has left.
/// </para>
/// </summary>
public sealed class OrgMemberHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="OrgMember"/>.</summary>
    public Guid EntityId { get; set; }

    // ---- Navigation ----

    public OrgMember Entity { get; set; } = null!;
}
