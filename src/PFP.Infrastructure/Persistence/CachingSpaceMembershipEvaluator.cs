using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using PFP.Application.Common.Interfaces;
using PFP.Infrastructure.Persistence.Configurations.Common;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Persistence;

/// <summary>
/// SQL Server–backed <see cref="SpaceRole"/> resolution with Redis / distributed-cache memoisation per spec Sprint 4.
/// </summary>
public sealed class CachingSpaceMembershipEvaluator : ISpaceMembershipEvaluator
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private static readonly DistributedCacheEntryOptions CacheOptions =
        new() { AbsoluteExpirationRelativeToNow = CacheTtl };

    private static readonly JsonSerializerOptions Serializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <remarks>
    /// <c>{"present":false}</c> negative cache hit.
    /// <c>{"present":true,"roleSnake":"manager"}</c> mirrors the textual DB enum.
    /// </remarks>
    private sealed record CachedPayload(bool Present, string? RoleSnake);

    private static readonly CachedPayload CachedAbsentPayload = new(false, null);
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    /// <summary>Creates the evaluator.</summary>
    public CachingSpaceMembershipEvaluator(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public async Task<SpaceRole?> GetEffectiveRoleAsync(
        Guid userId,
        Guid spaceId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(userId, spaceId);
        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        if (cachedBytes is not null)
            return DeserializeCached(cachedBytes);

        await using var connection = CreateConnection();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText =
            """
            SELECT TOP (1) [role]
              FROM space_members
             WHERE space_id = @space_id
               AND user_id = @user_id
               AND left_at IS NULL
               AND is_deleted = 0
            """;

        var pSid = cmd.CreateParameter();
        pSid.ParameterName = "@space_id";
        pSid.Value = spaceId;
        cmd.Parameters.Add(pSid);

        var pUid = cmd.CreateParameter();
        pUid.ParameterName = "@user_id";
        pUid.Value = userId;
        cmd.Parameters.Add(pUid);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        var scalar = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

        CachedPayload dto;
        if (scalar is not string snakeRoleRaw || string.IsNullOrWhiteSpace(snakeRoleRaw))
            dto = CachedAbsentPayload;
        else
            dto = new CachedPayload(true, snakeRoleRaw.Trim());

        await _cache.SetAsync(
                cacheKey,
                JsonSerializer.SerializeToUtf8Bytes(dto, Serializer),
                CacheOptions,
                cancellationToken)
            .ConfigureAwait(false);

        return dto.Present ? ParseRole(dto.RoleSnake!) : null;
    }

    /// <inheritdoc/>
    public Task InvalidateMembershipAsync(Guid userId, Guid spaceId, CancellationToken cancellationToken = default) =>
        _cache.RemoveAsync(BuildCacheKey(userId, spaceId), cancellationToken);

    /// <inheritdoc/>
    public async Task InvalidateMembershipBatchAsync(
        Guid userId,
        IEnumerable<Guid> spaceIds,
        CancellationToken cancellationToken = default)
    {
        foreach (var id in spaceIds.Distinct())
            await InvalidateMembershipAsync(userId, id, cancellationToken).ConfigureAwait(false);
    }

    private SqlConnection CreateConnection()
    {
        var cs = _configuration.GetConnectionString("Default")
                 ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");
        return new SqlConnection(cs);
    }

    private static string BuildCacheKey(Guid userId, Guid spaceId) =>
        $"space_member:{userId:N}:{spaceId:N}";

    private static SpaceRole? DeserializeCached(byte[] bytes)
    {
        var dto = JsonSerializer.Deserialize<CachedPayload>(bytes, Serializer)
                  ?? CachedAbsentPayload;
        return dto.Present ? ParseRole(dto.RoleSnake!) : null;
    }

    private static SpaceRole ParseRole(string snakeRole)
    {
        var pascal = SnakeCase.ToPascal(snakeRole.Trim());
        return Enum.TryParse(pascal, ignoreCase: false, out SpaceRole mapped)
            ? mapped
            : SpaceRole.Viewer;
    }
}
