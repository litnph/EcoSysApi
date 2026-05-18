using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Members.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.Members.RemoveOrgMember;

/// <summary>
/// Marks the member inactive AND ends any active space membership inside the same organisation
/// (sets <c>LeftAt</c>). The owner cannot be removed; ownership transfer is required first.
/// </summary>
public sealed class RemoveOrgMemberCommandHandler : IRequestHandler<RemoveOrgMemberCommand, RemoveOrgMemberResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public RemoveOrgMemberCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<RemoveOrgMemberResponse> Handle(RemoveOrgMemberCommand request, CancellationToken cancellationToken)
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
            throw new BusinessRuleException("Owner cannot be removed; transfer ownership first.");

        if (!member.IsActive)
            return new RemoveOrgMemberResponse(member.Id);

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        member.IsActive = false;
        member.LeftAt = now;

        var spaceIds = await _db.Spaces.AsNoTracking()
            .Where(s => s.OrgId == request.OrganizationId)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (spaceIds.Count > 0)
        {
            var spaceMemberships = await _db.SpaceMembers
                .Where(sm => sm.UserId == member.UserId
                             && spaceIds.Contains(sm.SpaceId)
                             && sm.LeftAt == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var sm in spaceMemberships)
                sm.LeftAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new RemoveOrgMemberResponse(member.Id);
    }
}
