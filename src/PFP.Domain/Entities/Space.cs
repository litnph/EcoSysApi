using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// A logical workspace inside an <see cref="Organization"/>, organised as a tree.
/// Maps to <c>SPACES</c>.
/// <para>
/// <b>Tree representation</b> (per spec §4.5):
/// <list type="bullet">
/// <item><see cref="ParentId"/> — self-FK; <c>null</c> for the root space of the organisation.</item>
/// <item><see cref="Path"/> — materialised-path string of the form
/// <c>"/{org.Id}"</c> for the root and <c>parent.Path + "/" + space.Id</c> for descendants.
/// Used for fast subtree queries (<c>WHERE path LIKE '/{root}/{branch}%'</c>).</item>
/// <item><see cref="Depth"/> — number of edges to the root; root carries depth <c>0</c>.</item>
/// <item><see cref="SortOrder"/> — ordering hint among siblings (UI only, no business meaning).</item>
/// </list>
/// On <c>MoveSpace</c> the handler must rewrite <see cref="Path"/> and <see cref="Depth"/> for the
/// entire affected subtree in a single batch update — see spec §4.5.
/// </para>
/// <para>
/// Indexes (configured in EF mapping per spec §3.8): single-column on <see cref="Path"/>,
/// composite on (<see cref="OrgId"/>, <see cref="ParentId"/>).
/// </para>
/// </summary>
public sealed class Space : VersionedEntity
{
    /// <summary>FK to <see cref="Organization"/>.</summary>
    public Guid OrgId { get; set; }

    /// <summary>Self-FK to the parent space; <c>null</c> for the organisation's root space.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Display name of the space.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional free-form description.</summary>
    public string? Description { get; set; }

    /// <summary>Type of space — drives UI affordances and feature gating.</summary>
    public SpaceType Type { get; set; } = SpaceType.Personal;

    /// <summary>
    /// Materialised-path string. Root: <c>"/{OrgId}"</c>. Descendants: <c>parent.Path + "/" + Id</c>.
    /// Maintained by the create/move handler — handlers MUST NOT compute it inline elsewhere.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>Depth in the tree (root = <c>0</c>).</summary>
    public int Depth { get; set; }

    /// <summary>Sibling sort order; ascending. Default <c>0</c>.</summary>
    public int SortOrder { get; set; }

    // ---- Navigation ----

    public Organization Org { get; set; } = null!;

    /// <summary>Parent space; <c>null</c> when this is the root.</summary>
    public Space? Parent { get; set; }

    /// <summary>Direct children of this space (one level down).</summary>
    public ICollection<Space> Children { get; set; } = new List<Space>();

    /// <summary>Members of this specific space (both directly added and inherited from ancestors).</summary>
    public ICollection<SpaceMember> Members { get; set; } = new List<SpaceMember>();

    /// <summary>Modules enabled for this space.</summary>
    public ICollection<SpaceModule> Modules { get; set; } = new List<SpaceModule>();

    /// <summary>Version history rows for this space (append-only).</summary>
    public ICollection<SpaceHistory> History { get; set; } = new List<SpaceHistory>();
}
