using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PFP.Application.Common.Options;

namespace PFP.API.Authorization;

/// <summary>Authorizes callers listed under <c>PlatformAdmin:UserIds</c>.</summary>
public sealed class PlatformAdminAuthorizationHandler : AuthorizationHandler<PlatformAdminRequirement>
{
    private readonly IOptions<PlatformAdminOptions> _options;

    /// <summary>Creates the handler.</summary>
    public PlatformAdminAuthorizationHandler(IOptions<PlatformAdminOptions> options) => _options = options;

    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PlatformAdminRequirement requirement)
    {
        var allow = _options.Value.UserIds;
        if (allow.Count == 0)
            return Task.CompletedTask;

        if (context.User.FindFirstValue(ClaimTypes.NameIdentifier) is not { } sub || !Guid.TryParse(sub, out var userId))
            return Task.CompletedTask;

        if (allow.Contains(userId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
