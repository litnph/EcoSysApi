using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Application.Features.Organizations.Members.Common;

namespace PFP.Application.Features.Organizations.Members.GetOrgMembers;

/// <summary>Returns the membership roster, joining <c>USERS</c> for display data.</summary>
public sealed class GetOrgMembersQueryHandler : IRequestHandler<GetOrgMembersQuery, GetOrgMembersResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetOrgMembersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetOrgMembersResponse> Handle(GetOrgMembersQuery request, CancellationToken cancellationToken)
    {
        await OrgMembersAuthorization
            .EnsureCanViewMembersAsync(_db, _currentUser, request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        var query = from m in _db.OrgMembers.AsNoTracking()
                    where m.OrgId == request.OrganizationId
                          && (request.IncludeInactive || m.IsActive)
                    join u in _db.Users.AsNoTracking() on m.UserId equals u.Id
                    orderby m.Role descending, u.FullName
                    select new OrgMemberDto(
                        m.Id,
                        m.UserId,
                        u.Email,
                        u.FullName,
                        m.Role,
                        m.IsActive,
                        m.JoinedAt,
                        m.LeftAt,
                        m.InvitedBy);

        var members = await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        return new GetOrgMembersResponse(members);
    }
}
