namespace PFP.Domain.Entities;

/// <summary>
/// Version-history row for <see cref="Space"/>. Maps to <c>SPACES_HISTORY</c> (spec §3.7).
/// <para>
/// Particularly important for the <c>MoveSpace</c> flow (spec §4.5) — every node whose
/// <see cref="Space.Path"/> or <see cref="Space.Depth"/> is rewritten by the batch update
/// produces one history row, so the previous tree shape can be reconstructed from the snapshot.
/// </para>
/// </summary>
public sealed class SpaceHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="Space"/>.</summary>
    public Guid EntityId { get; set; }

    // ---- Navigation ----

    public Space Entity { get; set; } = null!;
}
