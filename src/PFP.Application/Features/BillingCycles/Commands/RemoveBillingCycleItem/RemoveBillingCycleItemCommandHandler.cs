using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.RefreshBillingCycle;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.RemoveBillingCycleItem;

public sealed class RemoveBillingCycleItemCommandHandler
    : IRequestHandler<RemoveBillingCycleItemCommand, RefreshBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemoveBillingCycleItemCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RefreshBillingCycleResponse> Handle(
        RemoveBillingCycleItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var cycle = await _db.FinBillingCycles
                .Include(c => c.Source)
                .FirstOrDefaultAsync(c => c.Id == request.CycleId, ct)
                .ConfigureAwait(false);

            if (cycle is null)
                throw new NotFoundException("Billing cycle was not found.");

            if (cycle.Status != BillingCycleStatus.Open)
                throw new BusinessRuleException("Only an open billing cycle can be edited.");

            var item = await _db.FinBillingCycleItems
                .FirstOrDefaultAsync(
                    i => i.BillingCycleId == cycle.Id
                         && i.TransactionId == request.TransactionId
                         && i.RemovedAt == null,
                    ct)
                .ConfigureAwait(false);

            if (item is null)
                throw new NotFoundException("Transaction is not on this open statement.");

            item.RemovedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            await BillingCycleTotals.RecalculateAsync(cycle, _db, ct).ConfigureAwait(false);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            return new RefreshBillingCycleResponse(
                FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name),
                AddedCount: 0,
                SkippedCount: 0);
        }, cancellationToken).ConfigureAwait(false);
    }
}
