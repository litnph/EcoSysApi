using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.DeleteInstallmentPlan;

public sealed class DeleteInstallmentPlanCommandHandler
    : IRequestHandler<DeleteInstallmentPlanCommand, Unit>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteInstallmentPlanCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteInstallmentPlanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var plan = await _db.FinInstallmentPlans
                .Include(p => p.Pays)
                .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
                .ConfigureAwait(false);

            if (plan is null)
                throw new NotFoundException("Installment plan was not found.");

            if (plan.Status is not (InstallmentStatus.Active or InstallmentStatus.Completed))
                throw new BusinessRuleException("Only an active or completed installment plan can be deleted.");

            if (plan.Status == InstallmentStatus.Active && plan.Pays.Any(p => p.TxnId is not null))
                throw new BusinessRuleException(
                    "Cannot delete a plan that has installments paid through the app. Cancel the plan instead.");

            if (plan.ConversionFeeStatus is ConversionFeeStatus.Billed or ConversionFeeStatus.Paid)
                throw new BusinessRuleException("Cannot delete a plan whose conversion fee was already billed.");

            var cardSource = await _db.FinSources
                .FirstAsync(s => s.Id == plan.SourceId, ct)
                .ConfigureAwait(false);

            if (plan.Status == InstallmentStatus.Active)
            {
                var backfillToRestore = plan.Pays
                    .Where(p => p.Status == InstallmentPayStatus.Paid && p.TxnId is null)
                    .Sum(p => p.Amount);

                if (backfillToRestore > 0m)
                    cardSource.Balance += backfillToRestore;
            }
            else
            {
                var paymentTxnIds = plan.Pays
                    .Where(p => p.TxnId is not null)
                    .Select(p => p.TxnId!.Value)
                    .ToList();

                if (paymentTxnIds.Count > 0)
                {
                    var paymentTxns = await _db.FinTransactions
                        .Where(t => paymentTxnIds.Contains(t.Id))
                        .ToListAsync(ct)
                        .ConfigureAwait(false);

                    foreach (var txn in paymentTxns)
                        txn.InstallmentPlanId = null;
                }
            }

            _db.FinInstallmentPays.RemoveRange(plan.Pays);
            _db.FinInstallmentPlans.Remove(plan);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}
