using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.CloseBillingCycle;

/// <summary>Marks a cycle <see cref="BillingCycleStatus.Closed"/> and refreshes totals from deferred activity.</summary>
public sealed class CloseBillingCycleCommandHandler : IRequestHandler<CloseBillingCycleCommand, CloseBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CloseBillingCycleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<CloseBillingCycleResponse> Handle(CloseBillingCycleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var cycle = await _db.FinBillingCycles
                .Include(bc => bc.Source)
                .FirstOrDefaultAsync(bc => bc.Id == request.CycleId, ct)
                .ConfigureAwait(false);

            if (cycle is null)
                throw new NotFoundException("Billing cycle was not found.");

            if (cycle.Status != BillingCycleStatus.Open)
                throw new BusinessRuleException("Only an open billing cycle can be closed.");

            await BillingCycleTotals.RecalculateAsync(cycle, _db, ct).ConfigureAwait(false);
            cycle.Status = BillingCycleStatus.Closed;
            cycle.ClosedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return new CloseBillingCycleResponse(FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name));
        }, cancellationToken).ConfigureAwait(false);
    }
}
