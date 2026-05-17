using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.ToggleAutomationRule;

public sealed class ToggleAutomationRuleCommandHandler : IRequestHandler<ToggleAutomationRuleCommand, ToggleAutomationRuleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ToggleAutomationRuleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ToggleAutomationRuleResponse> Handle(ToggleAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.AutomationRules
            .Include(r => r.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Automation rule was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(entity.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage automation for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && entity.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this automation rule.");

        entity.IsActive = !entity.IsActive;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new ToggleAutomationRuleResponse(entity.Id, entity.IsActive);
    }
}
