using PFP.Domain.Enums;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Resolves the effective <see cref="SpaceRole"/> for a (<see cref="User"/>, <see cref="Space"/>)
/// tuple with distributed caching (<c>TTL≈5m</c>, key prefix <c>space_member:{user}:{space}</c>).
/// </summary>
public interface ISpaceMembershipEvaluator
{
    /// <summary>Returns the active tier when membership exists (direct or inherited); otherwise <c>null</c>.</summary>
    Task<SpaceRole?> GetEffectiveRoleAsync(
        Guid userId,
        Guid spaceId,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts cached membership outcome for one space.</summary>
    Task InvalidateMembershipAsync(
        Guid userId,
        Guid spaceId,
        CancellationToken cancellationToken = default);

    /// <summary>Evicts many spaces for the same user (batch invalidation).</summary>
    Task InvalidateMembershipBatchAsync(
        Guid userId,
        IEnumerable<Guid> spaceIds,
        CancellationToken cancellationToken = default);
}
