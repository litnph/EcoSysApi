using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Automation.DeleteAutomationRule;

public sealed class DeleteAutomationRuleCommandHandler : IRequestHandler<DeleteAutomationRuleCommand, DeleteAutomationRuleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteAutomationRuleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DeleteAutomationRuleResponse> Handle(DeleteAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.AutomationRules
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Automation rule was not found.");
await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        _db.AutomationRules.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new DeleteAutomationRuleResponse(request.Id);
    }
}
