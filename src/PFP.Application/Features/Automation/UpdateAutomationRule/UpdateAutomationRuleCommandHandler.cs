using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.UpdateAutomationRule;

public sealed class UpdateAutomationRuleCommandHandler : IRequestHandler<UpdateAutomationRuleCommand, UpdateAutomationRuleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateAutomationRuleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<UpdateAutomationRuleResponse> Handle(UpdateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Automation rule was not found.");
entity.Name = request.Name.Trim();
        entity.TriggerType = request.TriggerType;
        entity.TriggerValue = request.TriggerValue.Trim();
        entity.Conditions = request.Conditions.Trim();
        entity.Actions = request.Actions.Trim();
        entity.IsActive = request.IsActive;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new UpdateAutomationRuleResponse(AutomationRuleDtoMapper.ToDetail(entity));
    }
}
