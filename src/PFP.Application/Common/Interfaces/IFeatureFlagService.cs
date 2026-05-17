using PFP.Application.Common.Models;

namespace PFP.Application.Common.Interfaces;

/// <summary>Evaluates feature flags with rollout, overrides, archival rules, and short-lived Redis-backed caching.</summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Resolves one flag's effective value using override precedence (user, then org), rollout percentage,
    /// then global default. Cached per (<paramref name="key"/>, <paramref name="userId"/>).
    /// </summary>
    Task<bool> IsEnabledAsync(string key, Guid? userId, Guid? orgId, CancellationToken cancellationToken = default);

    /// <summary>Returns every persisted flag paired with its resolved enabled state — used on front-end bootstrap.</summary>
    Task<IReadOnlyList<FeatureFlagForPrincipalDto>> GetAllResolvedForPrincipalAsync(
        Guid? userId,
        Guid? orgId,
        CancellationToken cancellationToken = default);
}
