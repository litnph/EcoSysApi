using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.DeleteBillingCycle;

public sealed class DeleteBillingCycleCommandHandler
    : IRequestHandler<DeleteBillingCycleCommand, GenerateBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteBillingCycleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GenerateBillingCycleResponse> Handle(
        DeleteBillingCycleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var cycle = await _db.FinBillingCycles
            .Include(c => c.Source)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        if (cycle.Status != BillingCycleStatus.Open)
            throw new BusinessRuleException("Only an open billing cycle can be deleted.");

        if (cycle.PaidAmount > 0)
            throw new BusinessRuleException("Cannot delete a billing cycle that already has payments.");

        _db.FinBillingCycleItems.RemoveRange(cycle.Items);
        _db.FinBillingCycles.Remove(cycle);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GenerateBillingCycleResponse(
            FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name));
    }
}
