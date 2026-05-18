using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.Common;

/// <summary>Shared authorisation helpers for the organisation-membership endpoints.</summary>
internal static class OrgMembersAuthorization
{
    /// <summary>Throws when the current user cannot view memberships of the requested org.</summary>
    internal static async Task EnsureCanViewMembersAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        Guid orgId,
        CancellationToken cancellationToken)
    {
        if (!current.IsAuthenticated || current.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var uid = current.UserId.Value;
        var isMember = await db.OrgMembers.AsNoTracking()
            .AnyAsync(m => m.OrgId == orgId && m.UserId == uid && m.IsActive, cancellationToken)
            .ConfigureAwait(false);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this organisation.");
    }

    /// <summary>Throws when the current user cannot manage memberships of the requested org.</summary>
    internal static async Task EnsureCanManageMembersAsync(
        IApplicationDbContext db,
        ICurrentUserService current,
        Guid orgId,
        CancellationToken cancellationToken)
    {
        if (!current.IsAuthenticated || current.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var uid = current.UserId.Value;
        var elevated = await db.OrgMembers.AsNoTracking()
            .AnyAsync(
                m => m.OrgId == orgId
                     && m.UserId == uid
                     && m.IsActive
                     && (m.Role == OrgRole.Owner || m.Role == OrgRole.Admin),
                cancellationToken)
            .ConfigureAwait(false);

        if (!elevated)
            throw new ForbiddenException("Only organisation admins or owners can manage members.");
    }
}
