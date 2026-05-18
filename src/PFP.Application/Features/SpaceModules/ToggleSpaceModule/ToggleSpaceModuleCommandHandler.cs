using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.SpaceModules.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.SpaceModules.ToggleSpaceModule;

/// <summary>
/// Upserts the <c>SPACE_MODULES</c> row for <c>(space, module_code)</c>. Requires
/// <see cref="SpaceRole.Manager"/> in the target space (or org-level Admin / Owner).
/// </summary>
public sealed class ToggleSpaceModuleCommandHandler : IRequestHandler<ToggleSpaceModuleCommand, ToggleSpaceModuleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public ToggleSpaceModuleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<ToggleSpaceModuleResponse> Handle(ToggleSpaceModuleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var space = await _db.Spaces.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SpaceId, cancellationToken)
            .ConfigureAwait(false);

        if (space is null)
            throw new NotFoundException("Space was not found.");

        var orgElevated = await _currentUser
            .IsOrgMemberAsync(space.OrgId, OrgRole.Admin, cancellationToken)
            .ConfigureAwait(false);

        if (!orgElevated)
        {
            var managerOk = await _currentUser
                .IsSpaceMemberAsync(space.Id, SpaceRole.Manager, cancellationToken)
                .ConfigureAwait(false);

            if (!managerOk)
                throw new ForbiddenException("Only space managers or organisation admins can toggle modules.");
        }

        var existing = await _db.SpaceModules
            .FirstOrDefaultAsync(m => m.SpaceId == request.SpaceId && m.ModuleCode == request.ModuleCode, cancellationToken)
            .ConfigureAwait(false);

        var now = DateTime.UtcNow;
        SpaceModule module;

        if (existing is null)
        {
            module = new SpaceModule
            {
                SpaceId = request.SpaceId,
                ModuleCode = request.ModuleCode,
                IsEnabled = request.Enable,
                EnabledAt = request.Enable ? now : default,
                DisabledAt = request.Enable ? null : now,
            };
            _db.SpaceModules.Add(module);
        }
        else
        {
            module = existing;
            if (request.Enable)
            {
                if (!module.IsEnabled)
                {
                    module.IsEnabled = true;
                    module.EnabledAt = now;
                    module.DisabledAt = null;
                }
            }
            else
            {
                if (module.IsEnabled)
                {
                    module.IsEnabled = false;
                    module.DisabledAt = now;
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = new SpaceModuleDto(
            module.Id,
            module.SpaceId,
            module.ModuleCode,
            module.IsEnabled,
            module.Settings,
            module.EnabledAt,
            module.DisabledAt);

        return new ToggleSpaceModuleResponse(dto);
    }
}
