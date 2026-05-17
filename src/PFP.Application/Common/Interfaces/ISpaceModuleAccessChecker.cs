using PFP.Domain.Enums;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Database-backed authorisation helper for <see cref="Domain.Entities.SpaceModule"/> access.
/// <para>
/// Lives outside <see cref="ICurrentUserService"/> so EF Core interceptors (which depend on the
/// current-user abstraction) do not create a circular dependency with <see cref="IApplicationDbContext"/>.
/// </para>
/// </summary>
public interface ISpaceModuleAccessChecker
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="userId"/> is an active member of the parent space with
    /// at least <paramref name="minimumRole"/>, and the module row is enabled with <c>module_code = finance</c>.
    /// </summary>
    Task<bool> HasSpaceModuleAccessAsync(
        Guid userId,
        Guid smoduleId,
        SpaceRole minimumRole = SpaceRole.Viewer,
        CancellationToken cancellationToken = default);
}
