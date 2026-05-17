using PFP.Application.Common.Localization;
using PFP.Domain.Enums;

namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the authenticated user that owns the current HTTP / background-job request.
/// <para>
/// Per spec §6.2 every Application-layer handler that performs authorisation work depends on
/// this service. Concrete implementations:
/// <list type="bullet">
/// <item>HTTP context — reads the JWT claims attached to the request principal.</item>
/// <item>Hangfire job context — exposes the job-owning system user (<see cref="UserId"/> null,
/// <see cref="IsAuthenticated"/> false) so audit logs land in <c>SYSTEM_EVENT_LOGS</c> instead.</item>
/// <item>Test harness — supplies fixed values for unit / integration tests.</item>
/// </list>
/// </para>
/// </summary>
public interface ICurrentUserService
{
    /// <summary>FK to <c>USERS.id</c>; <c>null</c> for anonymous requests (login, register, password reset) and system jobs.</summary>
    Guid? UserId { get; }

    /// <summary>FK to <c>USER_SESSIONS.id</c>; <c>null</c> outside an authenticated HTTP context.</summary>
    Guid? SessionId { get; }

    /// <summary>FK to <c>ORGANIZATIONS.id</c> — the org the current request is acting inside. <c>null</c> for org-less endpoints.</summary>
    Guid? CurrentOrgId { get; }

    /// <summary><c>true</c> when both <see cref="UserId"/> and <see cref="SessionId"/> are present.</summary>
    bool IsAuthenticated { get; }

    /// <summary>IP address of the originating HTTP request, when available — used by the audit interceptor.</summary>
    string? IpAddress { get; }

    /// <summary>User-Agent header of the originating HTTP request, when available — used by the audit interceptor.</summary>
    string? UserAgent { get; }

    /// <summary>
    /// Returns <c>true</c> when the current user holds <paramref name="minimumRole"/> or higher in
    /// the requested organisation.
    /// </summary>
    Task<bool> IsOrgMemberAsync(Guid orgId, OrgRole minimumRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the current user holds <paramref name="minimumRole"/> or higher in
    /// the requested space (direct or inherited membership).
    /// </summary>
    Task<bool> IsSpaceMemberAsync(Guid spaceId, SpaceRole minimumRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the current user can use the given <see cref="Domain.Entities.SpaceModule"/>
    /// — i.e. they are an active member of the parent space with at least <paramref name="minimumRole"/>,
    /// the module is enabled, and (for finance) <c>module_code = finance</c>.
    /// </summary>
    Task<bool> HasSpaceModuleAccessAsync(
        Guid smoduleId,
        SpaceRole minimumRole = SpaceRole.Viewer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Locale code selected for the current HTTP request (<see cref="RequestLocalizationKeys.HttpContextLocaleItemKey"/>),
    /// or <c>vi</c> when the pipeline did not resolve one (background jobs, integration defaults).
    /// </summary>
    string CurrentLocale { get; }
}
