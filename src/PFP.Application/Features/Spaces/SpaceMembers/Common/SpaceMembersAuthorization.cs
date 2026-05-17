using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Spaces.SpaceMembers.Common;

internal static class SpaceMembersAuthorization
{
    internal static async Task EnsureCanManageMembershipsAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        Guid orgId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        if (!current.IsAuthenticated || current.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (current.CurrentOrgId is { } ctxOrg && ctxOrg != orgId)
            throw new UnauthorizedAppException("You cannot manage members outside the current organisation context.");

        var uid = current.UserId.Value;

        var orgElevated = await db.OrgMembers.AnyAsync(
                m => m.OrgId == orgId
                     && m.UserId == uid
                     && m.IsActive
                     && (m.Role == OrgRole.Owner || m.Role == OrgRole.Admin),
                cancellationToken)
            .ConfigureAwait(false);

        if (orgElevated)
            return;

        var isManager = await current.IsSpaceMemberAsync(spaceId, SpaceRole.Manager, cancellationToken)
            .ConfigureAwait(false);

        if (!isManager)
            throw new UnauthorizedAppException("You do not have permission to manage memberships for this space.");
    }

    internal static async Task EnsureCanViewMembershipsAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        Guid orgId,
        Guid spaceId,
        CancellationToken cancellationToken)
    {
        if (!current.IsAuthenticated || current.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (current.CurrentOrgId is { } ctxOrg && ctxOrg != orgId)
            throw new UnauthorizedAppException("You cannot inspect members outside the current organisation context.");

        var uid = current.UserId.Value;

        var orgElevated = await db.OrgMembers.AnyAsync(
                m => m.OrgId == orgId
                     && m.UserId == uid
                     && m.IsActive
                     && (m.Role == OrgRole.Owner || m.Role == OrgRole.Admin),
                cancellationToken)
            .ConfigureAwait(false);

        if (orgElevated)
            return;

        var viewer = await current.IsSpaceMemberAsync(spaceId, SpaceRole.Viewer, cancellationToken).ConfigureAwait(false);
        if (!viewer)
            throw new UnauthorizedAppException("You cannot read memberships for this space.");
    }
}
