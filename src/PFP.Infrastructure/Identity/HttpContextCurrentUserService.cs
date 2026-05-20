using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Localization;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.Identity;

/// <summary>Maps JWT claims on the HTTP principal to <see cref="ICurrentUserService"/>.</summary>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISpaceModuleAccessChecker _spaceModuleAccess;
    private readonly ISpaceMembershipEvaluator _spaceMembership;
    private readonly IAutomationExecutionImpersonation _automationImpersonation;

    /// <summary>Creates the service.</summary>
    public HttpContextCurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        ISpaceModuleAccessChecker spaceModuleAccess,
        ISpaceMembershipEvaluator spaceMembership,
        IAutomationExecutionImpersonation automationImpersonation)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _spaceModuleAccess = spaceModuleAccess;
        _spaceMembership = spaceMembership;
        _automationImpersonation = automationImpersonation;
    }

    /// <inheritdoc/>
    public Guid? UserId =>
        ReadGuid(ClaimTypes.NameIdentifier)
        ?? (_automationImpersonation.IsActive ? _automationImpersonation.UserId : null);

    /// <inheritdoc/>
    public Guid? SessionId =>
        ReadGuid(JwtClaimNames.SessionId)
        ?? (_automationImpersonation.IsActive ? _automationImpersonation.SessionId : null);

    /// <inheritdoc/>
    public Guid? CurrentOrgId =>
        ReadGuid(JwtClaimNames.OrgId)
        ?? (_automationImpersonation.IsActive ? _automationImpersonation.OrgId : null);

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        UserId is not null && SessionId is not null;

    /// <inheritdoc/>
    public string? IpAddress =>
        HttpRequestMetadataTruncation.TruncateIpAddress(
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString());

    /// <inheritdoc/>
    public string? UserAgent =>
        HttpRequestMetadataTruncation.TruncateUserAgent(
            _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString());

    /// <inheritdoc/>
    public string CurrentLocale =>
        _httpContextAccessor.HttpContext?.Items.TryGetValue(RequestLocalizationKeys.HttpContextLocaleItemKey, out var localeObj) == true
        && localeObj is string loc
        && !string.IsNullOrWhiteSpace(loc)
            ? loc
            : "vi";

    /// <inheritdoc/>
    public async Task<bool> IsOrgMemberAsync(
        Guid orgId,
        OrgRole minimumRole,
        CancellationToken cancellationToken = default)
    {
        if (UserId is not Guid uid)
            return false;

        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var role = await db.OrgMembers.AsNoTracking()
            .Where(m => m.OrgId == orgId && m.UserId == uid && m.IsActive)
            .Select(m => (OrgRole?)m.Role)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return role is not null && role.Value >= minimumRole;
    }

    /// <inheritdoc/>
    public async Task<bool> IsSpaceMemberAsync(
        Guid spaceId,
        SpaceRole minimumRole,
        CancellationToken cancellationToken = default)
    {
        if (UserId is not Guid uid)
            return false;

        var role = await _spaceMembership
            .GetEffectiveRoleAsync(uid, spaceId, cancellationToken)
            .ConfigureAwait(false);

        return role is not null && role.Value >= minimumRole;
    }

    /// <inheritdoc/>
    public Task<bool> HasSpaceModuleAccessAsync(
        Guid smoduleId,
        SpaceRole minimumRole = SpaceRole.Viewer,
        CancellationToken cancellationToken = default) =>
        UserId is { } userId
            ? _spaceModuleAccess.HasSpaceModuleAccessAsync(userId, smoduleId, minimumRole, cancellationToken)
            : Task.FromResult(false);

    private Guid? ReadGuid(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
