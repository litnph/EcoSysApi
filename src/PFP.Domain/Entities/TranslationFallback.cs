namespace PFP.Domain.Entities;

/// <summary>
/// One link in a per-locale fallback chain. Maps to <c>TRANSLATION_FALLBACKS</c>.
/// <para>
/// When a translation is requested for <see cref="LocaleCode"/> but is missing, the resolver walks
/// the rows ordered by <see cref="Priority"/> ascending (1 = first fallback) and returns the first hit.
/// Example chain for <c>ja</c>: <c>ja</c> → <c>en</c> (priority 1) → <c>vi</c> (priority 2).
/// </para>
/// <para>
/// A composite unique index on (<see cref="LocaleCode"/>, <see cref="Priority"/>) guarantees a
/// deterministic ordering inside each chain.
/// </para>
/// </summary>
public sealed class TranslationFallback : BaseEntity
{
    /// <summary>FK to <see cref="Locale.Code"/> — the primary locale this fallback applies to.</summary>
    public string LocaleCode { get; set; } = string.Empty;

    /// <summary>FK to <see cref="Locale.Code"/> — the locale to consult when the primary is missing a key.</summary>
    public string FallbackLocaleCode { get; set; } = string.Empty;

    /// <summary>Position in the chain (1-based, ascending).</summary>
    public int Priority { get; set; }

    // ---- Navigation ----

    public Locale Locale { get; set; } = null!;

    public Locale FallbackLocale { get; set; } = null!;
}
