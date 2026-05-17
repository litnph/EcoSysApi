using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.CreateAutomationRule;

public sealed class CreateAutomationRuleCommandHandler : IRequestHandler<CreateAutomationRuleCommand, CreateAutomationRuleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateAutomationRuleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CreateAutomationRuleResponse> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(request.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to manage automation for this module.");

        var smodule = await _db.SpaceModules
            .Include(m => m.Space)
            .FirstOrDefaultAsync(m => m.Id == request.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (smodule is null)
            throw new NotFoundException("Space module was not found.");

        if (smodule.ModuleCode != ModuleCode.Finance || !smodule.IsEnabled)
            throw new BusinessRuleException("Automation rules require an enabled finance module.");

        if (_currentUser.CurrentOrgId is { } orgId && smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this space module.");

        var entity = new AutomationRule
        {
            SmoduleId = request.SmoduleId,
            CreatedByUserId = _currentUser.UserId.Value,
            Name = request.Name.Trim(),
            TriggerType = request.TriggerType,
            TriggerValue = request.TriggerValue.Trim(),
            Conditions = request.Conditions.Trim(),
            Actions = request.Actions.Trim(),
            IsActive = true,
        };

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.AutomationRules.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CreateAutomationRuleResponse(AutomationRuleDtoMapper.ToDetail(entity));
    }
}
