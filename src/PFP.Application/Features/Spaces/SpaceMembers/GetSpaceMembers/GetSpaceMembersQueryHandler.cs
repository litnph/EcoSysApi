using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Spaces.SpaceMembers.Common;

namespace PFP.Application.Features.Spaces.SpaceMembers.GetSpaceMembers;

public sealed class GetSpaceMembersQueryHandler : IRequestHandler<GetSpaceMembersQuery, GetSpaceMembersResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetSpaceMembersQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetSpaceMembersResponse> Handle(GetSpaceMembersQuery request, CancellationToken cancellationToken)
    {
        var space = await _db.Spaces.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            .ConfigureAwait(false);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        await SpaceMembersAuthorization.EnsureCanViewMembershipsAsync(
                _db,
                _currentUser,
                space.OrgId,
                space.Id,
                cancellationToken)
            .ConfigureAwait(false);

        var rows = await (
                from m in _db.SpaceMembers.AsNoTracking()
                join u in _db.Users.AsNoTracking() on m.UserId equals u.Id
                where m.SpaceId == request.SpaceId && m.LeftAt == null
                orderby m.Inherited ascending, u.FullName
                select new SpaceMemberListDto(
                    u.Id,
                    u.Email,
                    u.FullName,
                    m.Role,
                    m.Inherited,
                    m.InheritedFromSpaceId,
                    m.InvitedBy,
                    m.JoinedAt))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new GetSpaceMembersResponse(rows);
    }
}
