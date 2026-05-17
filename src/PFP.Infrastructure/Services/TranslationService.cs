using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PFP.Application.Common.Interfaces;
using System.Text;

namespace PFP.Infrastructure.Services;

/// <inheritdoc />
public sealed class TranslationService : ITranslationService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IApplicationDbContext _db;
    private readonly IDistributedCache _cache;

    /// <summary>Creates the service.</summary>
    public TranslationService(IApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<string> GetTranslationAsync(
        string entityType,
        Guid entityId,
        string field,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildTranslationCacheKey(localeCode, entityType, entityId, field);
        var cached = await GetCachedStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        var value = await ResolveTranslationFromDatabaseAsync(entityType, entityId, field, localeCode, cancellationToken)
            .ConfigureAwait(false);

        await SetCachedStringAsync(cacheKey, value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    /// <inheritdoc />
    public async Task<string> GetUIStringAsync(
        string key,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildUiStringCacheKey(localeCode, key);
        var cached = await GetCachedStringAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached is not null)
            return cached;

        var value = await ResolveUiStringFromDatabaseAsync(key, localeCode, cancellationToken).ConfigureAwait(false);

        await SetCachedStringAsync(cacheKey, value, cancellationToken).ConfigureAwait(false);
        return value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>> GetTranslationsForEntityAsync(
        string entityType,
        Guid entityId,
        string localeCode,
        CancellationToken cancellationToken = default)
    {
        var fields = await _db.Translations.AsNoTracking()
            .Where(t => t.EntityType == entityType && t.EntityId == entityId)
            .Select(t => t.Field)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in fields)
        {
            dict[field] = await GetTranslationAsync(entityType, entityId, field, localeCode, cancellationToken)
                .ConfigureAwait(false);
        }

        return dict;
    }

    /// <inheritdoc />
    public Task RemoveTranslationCacheAsync(
        string localeCode,
        string entityType,
        Guid entityId,
        string field,
        CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(BuildTranslationCacheKey(localeCode, entityType, entityId, field), cancellationToken);

    /// <inheritdoc />
    public Task RemoveUiStringCacheAsync(string localeCode, string key, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(BuildUiStringCacheKey(localeCode, key), cancellationToken);

    private async Task<string> ResolveTranslationFromDatabaseAsync(
        string entityType,
        Guid entityId,
        string field,
        string localeCode,
        CancellationToken cancellationToken)
    {
        foreach (var candidate in await BuildLocaleCandidateChainAsync(localeCode, cancellationToken).ConfigureAwait(false))
        {
            var hit = await TryGetTranslationValueAsync(entityType, entityId, field, candidate, cancellationToken)
                .ConfigureAwait(false);
            if (hit is not null)
                return hit;
        }

        return string.Empty;
    }

    private async Task<string> ResolveUiStringFromDatabaseAsync(
        string key,
        string localeCode,
        CancellationToken cancellationToken)
    {
        foreach (var candidate in await BuildLocaleCandidateChainAsync(localeCode, cancellationToken).ConfigureAwait(false))
        {
            var hit = await TryGetUiStringValueAsync(key, candidate, cancellationToken).ConfigureAwait(false);
            if (hit is not null)
                return hit;
        }

        return string.Empty;
    }

    /// <summary>Requested locale first, then TRANSLATION_FALLBACKS rows, then platform default locale.</summary>
    private async Task<List<string>> BuildLocaleCandidateChainAsync(string localeCode, CancellationToken cancellationToken)
    {
        var chain = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !seen.Add(code))
                return;
            chain.Add(code);
        }

        Add(localeCode);

        var fallbacks = await _db.TranslationFallbacks.AsNoTracking()
            .Where(f => f.LocaleCode == localeCode)
            .OrderBy(f => f.Priority)
            .Select(f => f.FallbackLocaleCode)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var fb in fallbacks)
            Add(fb);

        var defaultLocale = await _db.Locales.AsNoTracking()
            .Where(l => l.IsDefault)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrEmpty(defaultLocale))
            Add(defaultLocale);

        return chain;
    }

    private async Task<string?> TryGetTranslationValueAsync(
        string entityType,
        Guid entityId,
        string field,
        string localeCode,
        CancellationToken cancellationToken)
    {
        var value = await _db.Translations.AsNoTracking()
            .Where(t =>
                t.EntityType == entityType
                && t.EntityId == entityId
                && t.Field == field
                && t.LocaleCode == localeCode)
            .Select(t => t.Value)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrEmpty(value) ? null : value;
    }

    private async Task<string?> TryGetUiStringValueAsync(string key, string localeCode, CancellationToken cancellationToken)
    {
        var value = await _db.UIStrings.AsNoTracking()
            .Where(s => s.Key == key && s.LocaleCode == localeCode)
            .Select(s => s.Value)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string BuildTranslationCacheKey(string localeCode, string entityType, Guid entityId, string field) =>
        $"trans:{localeCode}:{entityType}:{entityId}:{field}";

    private static string BuildUiStringCacheKey(string localeCode, string key) =>
        $"ui:{localeCode}:{key}";

    private async Task<string?> GetCachedStringAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var bytes = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    private Task SetCachedStringAsync(string cacheKey, string value, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl };
        return _cache.SetAsync(cacheKey, bytes, opts, cancellationToken);
    }
}
