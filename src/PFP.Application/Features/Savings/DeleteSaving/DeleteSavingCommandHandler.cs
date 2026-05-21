using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Savings.DeleteSaving;

public sealed class DeleteSavingCommandHandler : IRequestHandler<DeleteSavingCommand, DeleteSavingResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteSavingCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DeleteSavingResponse> Handle(DeleteSavingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinSavings
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Savings record was not found.");
if (entity.CurrentAmount != 0)
            throw new BusinessRuleException("Withdraw the savings balance before deleting this record.");

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinSavings.Remove(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new DeleteSavingResponse(request.Id);
    }
}
