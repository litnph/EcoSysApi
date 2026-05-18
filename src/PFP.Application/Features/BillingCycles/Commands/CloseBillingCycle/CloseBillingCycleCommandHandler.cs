using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
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

        await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var cycle = await _db.FinBillingCycles
            .Include(bc => bc.Source)
            .ThenInclude(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(bc => bc.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(cycle.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to close this billing cycle.");

        if (_currentUser.CurrentOrgId is { } orgId && cycle.Source.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this billing cycle.");

        if (cycle.Status != BillingCycleStatus.Open)
            throw new BusinessRuleException("Only an open billing cycle can be closed.");

        // Total charges in the cycle: deferred spends (reversals net out because they carry the same sign as the original row).
        var total = await _db.FinTransactions
            .AsNoTracking()
            .Where(t => t.BillingCycleId == cycle.Id && !t.IsDeleted && t.Type == TransactionType.Deferred)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        cycle.TotalAmount = total;
        cycle.Status = BillingCycleStatus.Closed;
        cycle.ClosedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new CloseBillingCycleResponse(FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name));
    }
}
