using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Investments.DeleteInvestment;

public sealed class DeleteInvestmentCommandHandler : IRequestHandler<DeleteInvestmentCommand, DeleteInvestmentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteInvestmentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DeleteInvestmentResponse> Handle(DeleteInvestmentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var entity = await _db.FinInvestments
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
            throw new NotFoundException("Investment was not found.");
await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
        _db.FinInvestments.Remove(entity);
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return new DeleteInvestmentResponse(request.Id);
    }
}
