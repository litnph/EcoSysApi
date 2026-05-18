using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.UpdateOrganization;

/// <summary>Persists organisation metadata changes; requires <see cref="OrgRole.Admin"/> or higher.</summary>
public sealed class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, UpdateOrganizationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public UpdateOrganizationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<UpdateOrganizationResponse> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var ok = await _currentUser.IsOrgMemberAsync(request.OrganizationId, OrgRole.Admin, cancellationToken).ConfigureAwait(false);
        if (!ok)
            throw new ForbiddenException("Only organisation admins or owners can update settings.");

        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken)
            .ConfigureAwait(false);

        if (org is null)
            throw new NotFoundException("Organisation was not found.");

        org.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.DefaultCurrency))
            org.DefaultCurrency = request.DefaultCurrency.Trim().ToUpperInvariant();
        org.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var memberCount = await _db.OrgMembers.AsNoTracking()
            .CountAsync(m => m.OrgId == org.Id && m.IsActive, cancellationToken)
            .ConfigureAwait(false);

        var myRole = await _db.OrgMembers.AsNoTracking()
            .Where(m => m.OrgId == org.Id && m.UserId == _currentUser.UserId.Value && m.IsActive)
            .Select(m => m.Role)
            .FirstAsync(cancellationToken)
            .ConfigureAwait(false);

        var dto = new OrganizationDetailDto(
            org.Id,
            org.Slug,
            org.Name,
            org.IsPersonal,
            org.OwnerId,
            org.DefaultCurrency,
            org.Description,
            myRole,
            memberCount,
            org.CreatedAt,
            org.UpdatedAt,
            org.Version);

        return new UpdateOrganizationResponse(dto);
    }
}
