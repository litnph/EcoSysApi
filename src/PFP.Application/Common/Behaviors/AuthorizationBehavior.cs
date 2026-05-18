using MediatR;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline stage that enforces the authorisation declared by
/// <see cref="IAuthorizeRequest"/> on a command or query (spec §2.3 / §6.2).
/// <para>
/// Performs three independent checks (skipping any that the request did not declare):
/// </para>
/// <list type="number">
/// <item>Authenticated session present in <see cref="ICurrentUserService"/>.</item>
/// <item><c>RequiredOrgId</c> + <c>MinimumOrgRole</c> via <see cref="ICurrentUserService.IsOrgMemberAsync"/>.</item>
/// <item><c>RequiredSpaceId</c> + <c>MinimumSpaceRole</c> via <see cref="ICurrentUserService.IsSpaceMemberAsync"/>.</item>
/// <item><c>RequiredSpaceModuleId</c> + <c>MinimumSpaceModuleRole</c> via <see cref="ICurrentUserService.HasSpaceModuleAccessAsync"/>.</item>
/// </list>
/// <para>
/// Throws <see cref="UnauthorizedAppException"/> on missing credentials and
/// <see cref="ForbiddenException"/> on role / membership mismatch — the API exception
/// middleware maps those to HTTP 401 and 403 respectively.
/// </para>
/// </summary>
/// <typeparam name="TRequest">Incoming command or query.</typeparam>
/// <typeparam name="TResponse">Handler return type.</typeparam>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the behaviour.</summary>
    public AuthorizationBehavior(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is IAuthorizeRequest auth)
        {
            if (auth.RequireAuthenticated && !_currentUser.IsAuthenticated)
                throw new UnauthorizedAppException("Authentication is required.");

            if (auth.RequiredOrgId is { } orgId)
            {
                var ok = await _currentUser.IsOrgMemberAsync(orgId, auth.MinimumOrgRole, cancellationToken)
                    .ConfigureAwait(false);
                if (!ok)
                    throw new ForbiddenException("You do not have the required organisation role.");
            }

            if (auth.RequiredSpaceId is { } spaceId)
            {
                var ok = await _currentUser.IsSpaceMemberAsync(spaceId, auth.MinimumSpaceRole, cancellationToken)
                    .ConfigureAwait(false);
                if (!ok)
                    throw new ForbiddenException("You do not have the required space role.");
            }

            if (auth.RequiredSpaceModuleId is { } smoduleId)
            {
                var ok = await _currentUser.HasSpaceModuleAccessAsync(
                        smoduleId,
                        auth.MinimumSpaceModuleRole,
                        cancellationToken)
                    .ConfigureAwait(false);
                if (!ok)
                    throw new ForbiddenException("You do not have access to this space module.");
            }
        }

        return await next().ConfigureAwait(false);
    }
}
