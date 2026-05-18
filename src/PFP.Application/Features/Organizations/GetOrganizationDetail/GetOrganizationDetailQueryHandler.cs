using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.GetOrganizationDetail;

/// <summary>Authorises a Member-or-higher caller and returns the organisation detail row.</summary>
public sealed class GetOrganizationDetailQueryHandler : IRequestHandler<GetOrganizationDetailQuery, GetOrganizationDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetOrganizationDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<GetOrganizationDetailResponse> Handle(GetOrganizationDetailQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var membership = await _db.OrgMembers.AsNoTracking()
            .Where(m => m.OrgId == request.OrganizationId && m.UserId == userId && m.IsActive)
            .Select(m => (OrgRole?)m.Role)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (membership is null)
            throw new ForbiddenException("You are not a member of this organisation.");

        var org = await _db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (org is null)
            throw new NotFoundException("Organisation was not found.");

        var memberCount = await _db.OrgMembers.AsNoTracking()
            .CountAsync(m => m.OrgId == org.Id && m.IsActive, cancellationToken)
            .ConfigureAwait(false);

        var dto = new OrganizationDetailDto(
            org.Id,
            org.Slug,
            org.Name,
            org.IsPersonal,
            org.OwnerId,
            org.DefaultCurrency,
            org.Description,
            membership.Value,
            memberCount,
            org.CreatedAt,
            org.UpdatedAt,
            org.Version);

        return new GetOrganizationDetailResponse(dto);
    }
}
