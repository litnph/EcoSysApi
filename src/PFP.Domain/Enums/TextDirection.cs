namespace PFP.Domain.Enums;

/// <summary>
/// Reading direction for a locale. Stored in <c>LOCALES.direction</c> and
/// returned to the client so the UI can flip layouts for RTL languages.
/// </summary>
public enum TextDirection
{
    /// <summary><c>ltr</c> — left-to-right (Latin, CJK, Cyrillic, …).</summary>
    Ltr = 1,

    /// <summary><c>rtl</c> — right-to-left (Arabic, Hebrew, Persian, …).</summary>
    Rtl = 2,
}
