namespace PFP.Domain.Entities;

/// <summary>
/// Version-history row for <see cref="Organization"/>. Maps to <c>ORGANIZATIONS_HISTORY</c> (spec §3.7).
/// <para>
/// Captures org-level changes — owner transfer, name / slug rename, default-currency switch,
/// soft-delete — written by the <c>HistoryInterceptor</c> in the same transaction as the originating
/// update on <c>ORGANIZATIONS</c>.
/// </para>
/// </summary>
public sealed class OrganizationHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="Organization"/>.</summary>
    public Guid EntityId { get; set; }

    // ---- Navigation ----

    public Organization Entity { get; set; } = null!;
}
