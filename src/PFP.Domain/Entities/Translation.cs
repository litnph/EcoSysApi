namespace PFP.Domain.Entities;

/// <summary>
/// Polymorphic translation of a single field on an arbitrary entity.
/// Maps to <c>TRANSLATIONS</c>.
/// <para>
/// The composite "natural key"
/// (<see cref="EntityType"/>, <see cref="EntityId"/>, <see cref="Field"/>, <see cref="LocaleCode"/>)
/// is enforced by a unique index — at most one translation row exists per entity / field / locale tuple.
/// </para>
/// <para>
/// Because the relation is polymorphic, no FK ties <see cref="EntityId"/> to a concrete table; integrity
/// is maintained at the application layer (translations are deleted alongside their owning entity).
/// </para>
/// </summary>
public sealed class Translation : BaseEntity
{
    /// <summary>CLR type name of the translated entity (e.g. <c>FinCategory</c>).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Identifier of the translated entity row.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Name of the field being translated (e.g. <c>Name</c>, <c>Description</c>).</summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>FK to <see cref="Locale.Code"/>.</summary>
    public string LocaleCode { get; set; } = string.Empty;

    /// <summary>Translated text in the target locale.</summary>
    public string Value { get; set; } = string.Empty;

    // ---- Navigation ----

    public Locale Locale { get; set; } = null!;
}
