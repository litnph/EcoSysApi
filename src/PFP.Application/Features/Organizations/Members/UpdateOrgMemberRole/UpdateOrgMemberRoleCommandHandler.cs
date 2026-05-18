using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Application.Features.Organizations.Members.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.UpdateOrgMemberRole;

/// <summary>
/// Mutates <c>OrgMember.Role</c>. Cannot demote the owner — for that flow the caller has to use
/// a separate ownership-transfer endpoint (not in scope of this command).
/// </summary>
public sealed class UpdateOrgMemberRoleCommandHandler : IRequestHandler<UpdateOrgMemberRoleCommand, UpdateOrgMemberRoleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateOrgMemberRoleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<UpdateOrgMemberRoleResponse> Handle(UpdateOrgMemberRoleCommand request, CancellationToken cancellationToken)
    {
        await OrgMembersAuthorization
            .EnsureCanManageMembersAsync(_db, _currentUser, request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        var member = await _db.OrgMembers
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.OrgId == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (member is null)
            throw new NotFoundException("Member was not found in this organisation.");

        if (member.Role == OrgRole.Owner)
            throw new BusinessRuleException("Owner role can only be changed via ownership transfer.");

        if (!member.IsActive)
            throw new BusinessRuleException("Cannot modify an inactive member.");

        member.Role = request.Role;
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == member.UserId)
            .Select(u => new { u.Email, u.FullName })
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        var dto = new OrgMemberDto(
            member.Id,
            member.UserId,
            user.Email,
            user.FullName,
            member.Role,
            member.IsActive,
            member.JoinedAt,
            member.LeftAt,
            member.InvitedBy);

        return new UpdateOrgMemberRoleResponse(dto);
    }
}
