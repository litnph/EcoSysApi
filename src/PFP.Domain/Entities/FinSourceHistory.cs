namespace PFP.Domain.Entities;

/// <summary>
/// Version-history row for <see cref="FinSource"/>. Maps to <c>FIN_SOURCES_HISTORY</c> (spec §3.7).
/// <para>
/// Most useful for tracing manual edits to credit-card configuration (statement day, due day,
/// credit limit). Balance is NOT updated in place per spec §4.6, so balance evolution is best
/// reconstructed from <see cref="FinTransaction"/> rows rather than from this table.
/// </para>
/// </summary>
public sealed class FinSourceHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="FinSource"/>.</summary>
    public Guid EntityId { get; set; }

    // ---- Navigation ----

    public FinSource Entity { get; set; } = null!;
}
