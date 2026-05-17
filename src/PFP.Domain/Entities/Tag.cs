namespace PFP.Domain.Entities;

/// <summary>User-defined finance tag scoped to one activated <see cref="SpaceModule"/> (<c>TAGS</c>).</summary>
public sealed class Tag : SoftDeletableEntity
{
    /// <summary><c>SPACE_MODULES.Id</c> — must be <see cref="Enums.ModuleCode.Finance"/> module.</summary>
    public Guid SmoduleId { get; set; }

    /// <summary>Display label unique per smodule.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>#RRGGBB hex foreground.</summary>
    public string Color { get; set; } = "#808080";

    /// <summary>Active <see cref="EntityTag"/> link count cached for delete guard.</summary>
    public int UsageCount { get; set; }

    /// <summary>Activated finance/home module backing this taxonomy.</summary>
    public SpaceModule Smodule { get; set; } = null!;

    /// <summary>Oriented links to aggregates.</summary>
    public ICollection<EntityTag> EntityTags { get; set; } = new List<EntityTag>();
}
