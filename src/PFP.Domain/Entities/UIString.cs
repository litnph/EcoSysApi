namespace PFP.Domain.Entities;

/// <summary>
/// Admin-managed UI label. Maps to <c>UI_STRINGS</c>.
/// <para>
/// Unlike <see cref="Translation"/>, which translates user-generated rows, this table holds
/// the framework-level labels that the front-end requests by <see cref="Key"/>
/// (e.g. <c>common.save</c>, <c>auth.login_failed</c>).
/// A composite unique index on (<see cref="Key"/>, <see cref="LocaleCode"/>) prevents duplicates.
/// </para>
/// </summary>
public sealed class UIString : BaseEntity
{
    /// <summary>Dot-separated label key — e.g. <c>common.save</c>, <c>auth.login_failed</c>.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>FK to <see cref="Locale.Code"/>.</summary>
    public string LocaleCode { get; set; } = string.Empty;

    /// <summary>Translated value displayed in the UI for this key/locale pair.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Optional translator-facing context describing where the label is used.</summary>
    public string? Description { get; set; }

    // ---- Navigation ----

    public Locale Locale { get; set; } = null!;
}
