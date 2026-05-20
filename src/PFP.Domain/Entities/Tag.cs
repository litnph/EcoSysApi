namespace PFP.Domain.Entities;

/// <summary>User-defined finance tag (<c>TAGS</c>).</summary>
public sealed class Tag : SoftDeletableEntity
{
    /// <summary>Display label unique across the app.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>#RRGGBB hex foreground.</summary>
    public string Color { get; set; } = "#808080";

    /// <summary>Active <see cref="EntityTag"/> link count cached for delete guard.</summary>
    public int UsageCount { get; set; }

    public ICollection<EntityTag> EntityTags { get; set; } = new List<EntityTag>();
}
