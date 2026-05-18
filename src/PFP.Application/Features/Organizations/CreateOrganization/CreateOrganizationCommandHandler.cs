using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Organizations.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Organizations.CreateOrganization;

/// <summary>
/// Creates an organisation, an <c>OrgMember</c> row promoting the caller to <see cref="OrgRole.Owner"/>,
/// a root <see cref="Space"/> ("General") and a default finance <see cref="SpaceModule"/> in one
/// DB transaction.
/// </summary>
public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, CreateOrganizationResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateOrganizationCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc cref="IRequestHandler{CreateOrganizationCommand, CreateOrganizationResponse}.Handle" />
    public async Task<CreateOrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var userId = _currentUser.UserId.Value;

        var slug = request.Slug.Trim().ToLowerInvariant();
        var slugTaken = await _db.Organizations.AnyAsync(o => o.Slug == slug, cancellationToken).ConfigureAwait(false);
        if (slugTaken)
            throw new BusinessRuleException("This organisation slug is already taken.");

        var currency = string.IsNullOrWhiteSpace(request.DefaultCurrency)
            ? "VND"
            : request.DefaultCurrency.Trim().ToUpperInvariant();

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var org = new Organization
        {
            IsPersonal = false,
            Slug = slug,
            Name = request.Name.Trim(),
            OwnerId = userId,
            DefaultCurrency = currency,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
        };
        _db.Organizations.Add(org);

        var member = new OrgMember
        {
            OrgId = org.Id,
            UserId = userId,
            Role = OrgRole.Owner,
            IsActive = true,
            JoinedAt = now,
        };
        _db.OrgMembers.Add(member);

        var rootSpace = new Space
        {
            OrgId = org.Id,
            ParentId = null,
            Name = "General",
            Type = SpaceType.Family,
            Path = $"/{org.Id}",
            Depth = 0,
            SortOrder = 0,
        };
        _db.Spaces.Add(rootSpace);

        var financeModule = new SpaceModule
        {
            SpaceId = rootSpace.Id,
            ModuleCode = ModuleCode.Finance,
            IsEnabled = true,
            EnabledAt = now,
        };
        _db.SpaceModules.Add(financeModule);

        var spaceMember = new SpaceMember
        {
            SpaceId = rootSpace.Id,
            UserId = userId,
            Role = SpaceRole.Manager,
            Inherited = false,
            JoinedAt = now,
        };
        _db.SpaceMembers.Add(spaceMember);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        var dto = new OrganizationDetailDto(
            org.Id,
            org.Slug,
            org.Name,
            org.IsPersonal,
            org.OwnerId,
            org.DefaultCurrency,
            org.Description,
            OrgRole.Owner,
            MemberCount: 1,
            org.CreatedAt,
            org.UpdatedAt,
            org.Version);

        return new CreateOrganizationResponse(dto);
    }
}
