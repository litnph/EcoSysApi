using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.CancelInstallmentPlan;

/// <summary>Marks a plan and its unpaid installments as cancelled.</summary>
public sealed class CancelInstallmentPlanCommandHandler : IRequestHandler<CancelInstallmentPlanCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CancelInstallmentPlanCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<Unit> Handle(CancelInstallmentPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var plan = await _db.FinInstallmentPlans
            .Include(p => p.Pays)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            .ConfigureAwait(false);

        if (plan is null || plan.Status != InstallmentStatus.Active)
            throw new NotFoundException("Installment plan was not found.");
plan.Status = InstallmentStatus.Cancelled;
        plan.CancellationReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        foreach (var pay in plan.Pays)
        {
            if (pay.Status == InstallmentPayStatus.Paid)
                continue;

            if (pay.Status is InstallmentPayStatus.Upcoming or InstallmentPayStatus.Due or InstallmentPayStatus.Overdue)
                pay.Status = InstallmentPayStatus.Upcoming;
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
