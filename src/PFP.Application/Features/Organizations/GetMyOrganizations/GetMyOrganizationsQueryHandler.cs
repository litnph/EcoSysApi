using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;

namespace PFP.Application.Features.Organizations.GetMyOrganizations;

/// <summary>Returns the caller's organisations together with their role inside each one.</summary>
public sealed class GetMyOrganizationsQueryHandler : IRequestHandler<GetMyOrganizationsQuery, GetMyOrganizationsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetMyOrganizationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetMyOrganizationsResponse> Handle(GetMyOrganizationsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var rows = await (
            from m in _db.OrgMembers.AsNoTracking()
            where m.UserId == userId && m.IsActive
            join o in _db.Organizations.AsNoTracking() on m.OrgId equals o.Id
            select new
            {
                o.Id,
                o.Slug,
                o.Name,
                o.IsPersonal,
                o.DefaultCurrency,
                m.Role,
                o.CreatedAt,
                MemberCount = _db.OrgMembers.AsNoTracking().Count(x => x.OrgId == o.Id && x.IsActive),
            })
            .OrderByDescending(x => x.IsPersonal)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = rows
            .Select(r => new OrganizationListItemDto(
                r.Id,
                r.Slug,
                r.Name,
                r.IsPersonal,
                r.DefaultCurrency,
                r.Role,
                r.MemberCount,
                r.CreatedAt))
            .ToList();

        return new GetMyOrganizationsResponse(items);
    }
}
