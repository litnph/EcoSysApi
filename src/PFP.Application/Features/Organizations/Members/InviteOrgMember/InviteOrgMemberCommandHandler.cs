using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Application.Features.Organizations.Members.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Organizations.Members.InviteOrgMember;

/// <summary>
/// Persists a new <see cref="OrgMember"/> row (or reactivates a previously inactive one).
/// Personal organisations cannot be invited into.
/// </summary>
public sealed class InviteOrgMemberCommandHandler : IRequestHandler<InviteOrgMemberCommand, InviteOrgMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public InviteOrgMemberCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<InviteOrgMemberResponse> Handle(InviteOrgMemberCommand request, CancellationToken cancellationToken)
    {
        await OrgMembersAuthorization
            .EnsureCanManageMembersAsync(_db, _currentUser, request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (org is null)
            throw new NotFoundException("Organisation was not found.");

        if (org.IsPersonal)
            throw new BusinessRuleException("Personal organisations cannot have additional members.");

        var invitee = await _db.Users.AsNoTracking()
            .Where(u => u.Id == request.UserId && u.IsActive && !u.IsDeleted)
            .Select(u => new { u.Id, u.Email, u.FullName })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (invitee is null)
            throw new NotFoundException("Invitee was not found.");

        var existing = await _db.OrgMembers
            .FirstOrDefaultAsync(m => m.OrgId == org.Id && m.UserId == request.UserId, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        OrgMember member;

        if (existing is null)
        {
            member = new OrgMember
            {
                OrgId = org.Id,
                UserId = request.UserId,
                Role = request.Role,
                IsActive = true,
                JoinedAt = now,
                InvitedBy = _currentUser.UserId,
            };
            _db.OrgMembers.Add(member);
        }
        else
        {
            if (existing.IsActive)
                throw new BusinessRuleException("This user is already an organisation member.");
            existing.IsActive = true;
            existing.Role = request.Role;
            existing.JoinedAt = now;
            existing.LeftAt = null;
            existing.InvitedBy = _currentUser.UserId;
            member = existing;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = new OrgMemberDto(
            member.Id,
            member.UserId,
            invitee.Email,
            invitee.FullName,
            member.Role,
            member.IsActive,
            member.JoinedAt,
            member.LeftAt,
            member.InvitedBy);

        return new InviteOrgMemberResponse(dto);
    }
}
