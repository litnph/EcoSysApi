using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Supported language / locale. Maps to <c>LOCALES</c>.
/// <para>
/// Acts as the i18n root: every translatable row in <see cref="Translation"/>,
/// every UI label in <see cref="UIString"/> and every fallback chain in
/// <see cref="TranslationFallback"/> references a locale by its <see cref="Code"/>.
/// </para>
/// <para>
/// Exactly one row may carry <see cref="IsDefault"/> = <c>true</c>; this is the locale used when
/// no per-user preference and no fallback chain produces a translation. The MVP defaults to <c>vi</c>.
/// </para>
/// </summary>
public sealed class Locale : BaseEntity
{
    /// <summary>BCP-47 identifier — e.g. <c>vi</c>, <c>en</c>, <c>en-US</c>, <c>ja</c>. Globally unique.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Native name of the language ("Tiếng Việt", "English", "日本語").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>English name of the language ("Vietnamese", "English", "Japanese").</summary>
    public string EnglishName { get; set; } = string.Empty;

    /// <summary>Reading direction — drives the front-end <c>dir</c> attribute on the <c>html</c> element.</summary>
    public TextDirection Direction { get; set; } = TextDirection.Ltr;

    /// <summary><c>true</c> on exactly one row, used as the platform-wide fallback locale.</summary>
    public bool IsDefault { get; set; }

    /// <summary>Soft-disable flag; an inactive locale cannot be selected by users but existing rows keep working.</summary>
    public bool IsActive { get; set; } = true;

    // ---- Navigation ----

    /// <summary>All translation rows targeting this locale.</summary>
    public ICollection<Translation> Translations { get; set; } = new List<Translation>();

    /// <summary>All UI string rows targeting this locale.</summary>
    public ICollection<UIString> UIStrings { get; set; } = new List<UIString>();

    /// <summary>Fallback chain entries where this locale is the primary side.</summary>
    public ICollection<TranslationFallback> Fallbacks { get; set; } = new List<TranslationFallback>();
}
