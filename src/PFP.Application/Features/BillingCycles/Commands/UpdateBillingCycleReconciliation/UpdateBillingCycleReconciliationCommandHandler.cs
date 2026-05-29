using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;
using PFP.Application.Features.BillingCycles.Common;

namespace PFP.Application.Features.BillingCycles.Commands.UpdateBillingCycleReconciliation;

public sealed class UpdateBillingCycleReconciliationCommandHandler
    : IRequestHandler<UpdateBillingCycleReconciliationCommand, GenerateBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateBillingCycleReconciliationCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<GenerateBillingCycleResponse> Handle(
        UpdateBillingCycleReconciliationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var cycle = await _db.FinBillingCycles
            .Include(c => c.Source)
            .FirstOrDefaultAsync(c => c.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        var note = request.ReconciliationNote?.Trim();
        cycle.ReconciliationNote = string.IsNullOrEmpty(note) ? null : note;
        cycle.IssuerStatementAmount = request.IssuerStatementAmount is { } whole
            ? CurrencyUnits.FromWhole(whole)
            : null;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new GenerateBillingCycleResponse(
            FinBillingCycleDtoMapper.ToDto(cycle, cycle.Source.Name));
    }
}
