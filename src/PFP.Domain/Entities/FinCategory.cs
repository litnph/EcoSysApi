using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Classification node for <see cref="FinTransaction"/> rows. Maps to <c>FIN_CATEGORIES</c>.
/// <para>
/// Tree: <see cref="ParentId"/> is optional; children must share the same <see cref="Kind"/> as the parent.
/// </para>
/// <para>
/// <see cref="Code"/> is a stable unique key (machine-generated or seeded).
/// </para>
/// <para>
/// <see cref="IsSystem"/> marks platform-seeded rows that user flows may protect from rename/delete.
/// </para>
/// </summary>
public sealed class FinCategory : SoftDeletableEntity
{
    /// <summary>Display name (max 100 in EF mapping).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Stable machine code — unique across the app.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Money-flow bucket for this category.</summary>
    public CategoryKind Kind { get; set; } = CategoryKind.Expense;

    /// <summary>Optional icon key.</summary>
    public string? Icon { get; set; }

    /// <summary>Optional accent colour (hex).</summary>
    public string? Color { get; set; }

    /// <summary>Self-FK for tree layout; <c>null</c> for root categories.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Materialised path for subtree queries (optional).</summary>
    public string? Path { get; set; }

    /// <summary>Depth in the tree (root = <c>0</c>).</summary>
    public int Depth { get; set; }

    /// <summary>UI sort among siblings.</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// When <c>true</c>, this category is the default pick for its <see cref="Kind"/> in UI (only one default per kind per module is enforced in handlers).
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary><c>true</c> for platform-seeded categories.</summary>
    public bool IsSystem { get; set; }

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    // ---- Navigation ----
    public FinCategory? Parent { get; set; }

    public ICollection<FinCategory> Children { get; set; } = new List<FinCategory>();

    public ICollection<FinTransaction> Transactions { get; set; } = new List<FinTransaction>();
}
