namespace PFP.Domain.Entities;

/// <summary>Join row linking one <see cref="Tag"/> to a finance entity (<c>entity_tags</c>).</summary>
public sealed class EntityTag : SoftDeletableEntity
{
    public Guid TagId { get; set; }

    /// <summary>CLR entity name anchor (e.g. <see cref="FinTransaction"/>).</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    /// <summary>User who tagged the row (<c>users.id</c>).</summary>
    public Guid TaggedBy { get; set; }

    public Tag Tag { get; set; } = null!;
    public User Tagger { get; set; } = null!;
}
