namespace PFP.Domain.Entities;

/// <summary>
/// Version-history row for <see cref="SpaceMember"/>. Maps to <c>SPACE_MEMBERS_HISTORY</c> (spec §3.7).
/// <para>
/// Captures role changes plus the cascade rewrites triggered by <c>MoveSpace</c> (spec §4.5),
/// where inherited memberships move with the subtree.
/// </para>
/// </summary>
public sealed class SpaceMemberHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="SpaceMember"/>.</summary>
    public Guid EntityId { get; set; }

    // ---- Navigation ----

    public SpaceMember Entity { get; set; } = null!;
}
