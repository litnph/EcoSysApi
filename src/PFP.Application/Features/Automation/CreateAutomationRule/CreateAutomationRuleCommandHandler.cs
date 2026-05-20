using MediatR;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Automation.Common;
using PFP.Domain.Entities;

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

        var entity = new AutomationRule
        {
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
