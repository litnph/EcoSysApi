using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Models;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Services;

/// <inheritdoc cref="IFeatureFlagService" />
public sealed class FeatureFlagService : IFeatureFlagService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IApplicationDbContext _db;
    private readonly IDistributedCache _cache;

    /// <summary>Creates the evaluator.</summary>
    public FeatureFlagService(IApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEnabledAsync(string key, Guid? userId, Guid? orgId, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(key, userId);
        var cached = await TryGetCachedBoolAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cached.HasValue)
            return cached.Value;

        var computed = await EvaluateSingleAsync(key, userId, orgId, cancellationToken).ConfigureAwait(false);

        await SetCachedBoolAsync(cacheKey, computed, cancellationToken).ConfigureAwait(false);
        return computed;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<FeatureFlagForPrincipalDto>> GetAllResolvedForPrincipalAsync(
        Guid? userId,
        Guid? orgId,
        CancellationToken cancellationToken = default)
    {
        var flags = await _db.FeatureFlags.AsNoTracking()
            .OrderBy(f => f.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (flags.Count == 0)
            return [];

        var flagIds = flags.Select(f => f.Id).ToList();
        var utcNow = DateTime.UtcNow;

        var overrides = await _db.FeatureFlagOverrides.AsNoTracking()
            .Where(o => flagIds.Contains(o.FlagId))
            .Where(o => o.ExpiresAt == null || o.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var byFlagId = overrides
            .GroupBy(o => o.FlagId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<FeatureFlagOverride>)g.ToList());

        var result = new List<FeatureFlagForPrincipalDto>(flags.Count);

        foreach (var flag in flags)
        {
            byFlagId.TryGetValue(flag.Id, out var list);
            list ??= Array.Empty<FeatureFlagOverride>();

            var enabled = EvaluateFlag(flag, list, userId, orgId);

            result.Add(new FeatureFlagForPrincipalDto(
                flag.Id,
                flag.Key,
                flag.Name,
                flag.Description,
                flag.IsEnabledGlobal,
                flag.RolloutPercentage,
                flag.IsArchived,
                enabled));
        }

        return result;
    }

    private async Task<bool> EvaluateSingleAsync(string key, Guid? userId, Guid? orgId, CancellationToken cancellationToken)
    {
        var flag = await _db.FeatureFlags.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Key == key, cancellationToken)
            .ConfigureAwait(false);

        if (flag is null)
            return false;

        var utcNow = DateTime.UtcNow;
        var overrides = await _db.FeatureFlagOverrides.AsNoTracking()
            .Where(o => o.FlagId == flag.Id && (o.ExpiresAt == null || o.ExpiresAt > utcNow))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return EvaluateFlag(flag, overrides, userId, orgId);
    }

    private static bool EvaluateFlag(
        FeatureFlag flag,
        IReadOnlyList<FeatureFlagOverride> overrides,
        Guid? userId,
        Guid? orgId)
    {
        if (flag.IsArchived)
            return false;

        IReadOnlyList<FeatureFlagOverride> ranked =
            overrides.Count <= 1
                ? overrides
                : overrides.OrderByDescending(o => o.CreatedAt).ToList();

        if (userId is { } uid)
        {
            foreach (var o in ranked)
            {
                if (o.TargetType == OverrideTargetType.User && o.TargetId == uid)
                    return o.IsEnabled;
            }
        }

        if (orgId is { } oid)
        {
            foreach (var o in ranked)
            {
                if (o.TargetType == OverrideTargetType.Org && o.TargetId == oid)
                    return o.IsEnabled;
            }
        }

        if (flag.RolloutPercentage > 0 && userId is { } rollUid && UserRolloutBucket(rollUid) < flag.RolloutPercentage)
            return true;

        return flag.IsEnabledGlobal;
    }

    private static int UserRolloutBucket(Guid userId)
    {
        Span<byte> guidBytes = stackalloc byte[16];
        _ = userId.TryWriteBytes(guidBytes);
        var hash = SHA256.HashData(guidBytes);
        var v = BinaryPrimitives.ReadUInt32LittleEndian(hash);
        return (int)(v % 100);
    }

    private static string BuildCacheKey(string flagKey, Guid? userId) =>
        $"ff:{flagKey}:{userId?.ToString("D") ?? ""}";

    private async Task<bool?> TryGetCachedBoolAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var bytes = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (bytes is null)
            return null;

        var s = Encoding.UTF8.GetString(bytes);
        return bool.TryParse(s, out var parsed) ? parsed : null;
    }

    private Task SetCachedBoolAsync(string cacheKey, bool value, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(value ? "true" : "false");
        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl };
        return _cache.SetAsync(cacheKey, bytes, opts, cancellationToken);
    }
}
