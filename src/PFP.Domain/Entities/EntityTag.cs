namespace PFP.Domain.Entities;

/// <summary>Join row linking one <see cref="Tag"/> to an arbitrary anchored entity (<c>ENTITY_TAGS</c>).</summary>
public sealed class EntityTag : SoftDeletableEntity
{
    public Guid TagId { get; set; }

    /// <summary>Module discriminator (typically <c>finance</c>).</summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>CLR entity name anchor (e.g. <see cref="FinTransaction"/>).</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>User who tagged the row (<c>USERS.id</c>).</summary>
    public Guid TaggedBy { get; set; }

    // ---- Navigation ----

    public Tag Tag { get; set; } = null!;
    public User Tagger { get; set; } = null!;
}
