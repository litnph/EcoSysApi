namespace PFP.Application.Common.Interfaces;

/// <summary>Resolves entity field translations and admin UI strings with locale fallback and distributed caching.</summary>
public interface ITranslationService
{
    /// <summary>Resolves a single polymorphic field using <c>TRANSLATIONS</c> + <c>TRANSLATION_FALLBACKS</c>.</summary>
    Task<string> GetTranslationAsync(
        string entityType,
        Guid entityId,
        string field,
        string localeCode,
        CancellationToken cancellationToken = default);

    /// <summary>Resolves a UI label from <c>UI_STRINGS</c> using the same fallback chain as entity translations.</summary>
    Task<string> GetUIStringAsync(
        string key,
        string localeCode,
        CancellationToken cancellationToken = default);

    /// <summary>Maps field name → resolved text for one entity and locale (fallback-aware per field).</summary>
    Task<IReadOnlyDictionary<string, string>> GetTranslationsForEntityAsync(
        string entityType,
        Guid entityId,
        string localeCode,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts the Redis / distributed-cache entry for one entity field (call after admin edits).</summary>
    Task RemoveTranslationCacheAsync(
        string localeCode,
        string entityType,
        Guid entityId,
        string field,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts cached UI string entries for the key across all locales that might have been requested.</summary>
    Task RemoveUiStringCacheAsync(
        string localeCode,
        string key,
        CancellationToken cancellationToken = default);
}
