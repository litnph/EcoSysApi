using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.CreateInstallmentPlan;

/// <summary>Creates an installment plan and its pay schedule in one database transaction.</summary>
public sealed class CreateInstallmentPlanCommandHandler : IRequestHandler<CreateInstallmentPlanCommand, CreateInstallmentPlanResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public CreateInstallmentPlanCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<CreateInstallmentPlanResponse> Handle(
        CreateInstallmentPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var txn = await _db.FinTransactions
            .Include(t => t.Source)
            .FirstOrDefaultAsync(t => t.Id == request.OriginalTxnId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null || txn.IsDeleted || txn.Type != TransactionType.Deferred)
            throw new NotFoundException("Transaction was not found.");
if (txn.Source.Type != SourceType.CreditCard)
            throw new BusinessRuleException("Installment plans can only be created for credit card sources.");

        var totalAmount = txn.Amount;
        decimal? conversionFeeAmount = null;
        ConversionFeeStatus? conversionFeeStatus = null;
        if (request.ConversionFeeRate is { } rate && rate > 0)
        {
            conversionFeeAmount = decimal.Round(totalAmount * rate / 100m, 2, MidpointRounding.AwayFromZero);
            conversionFeeStatus = ConversionFeeStatus.Pending;
        }

        var totalMonths = request.TotalMonths;
        var (monthlyShare, lastShare) = InstallmentScheduleSplit.Split(totalAmount, totalMonths);

        var startDate = txn.TxnDate;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Wrap explicit transaction with the retrying execution strategy so EnableRetryOnFailure
        // can retry transient transport faults around the plan + schedule write.
        var strategy = _db.Database.CreateExecutionStrategy();
        var planId = await strategy.ExecuteAsync(async () =>
        {
            await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var plan = new FinInstallmentPlan
            {                OriginalTxnId = txn.Id,
                SourceId = txn.SourceId,
                TotalAmount = totalAmount,
                TotalMonths = totalMonths,
                MonthlyAmount = monthlyShare,
                InterestRate = request.InterestRate,
                ConversionFeeRate = request.ConversionFeeRate,
                ConversionFeeAmount = conversionFeeAmount,
                ConversionFeeStatus = conversionFeeStatus,
                StartDate = startDate,
                Status = InstallmentStatus.Active,
            };

            _db.FinInstallmentPlans.Add(plan);

            if (txn.Status == TxnStatus.New)
                txn.Status = TxnStatus.TransferredToInstallment;

            var cardSource = await _db.FinSources
                .FirstAsync(s => s.Id == txn.SourceId, cancellationToken)
                .ConfigureAwait(false);

            var pays = new List<FinInstallmentPay>(totalMonths);
            for (var i = 1; i <= totalMonths; i++)
            {
                var amount = i == totalMonths ? lastShare : monthlyShare;
                var dueDate = InstallmentPaySchedule.DueDateForInstallment(startDate, i);
                var pay = new FinInstallmentPay
                {
                    PlanId = plan.Id,
                    InstallmentNumber = i,
                };
                InstallmentPaySchedule.ApplyInitialPayLine(pay, amount, dueDate, today);
                pays.Add(pay);
                _db.FinInstallmentPays.Add(pay);
            }

            if (InstallmentPaySchedule.IsFullyPaid(pays))
                plan.Status = InstallmentStatus.Completed;

            var backfillPaidTotal = pays
                .Where(p => p.Status == InstallmentPayStatus.Paid)
                .Sum(p => p.Amount);

            if (backfillPaidTotal > 0m)
                cardSource.Balance = Math.Max(0m, cardSource.Balance - backfillPaidTotal);

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return plan.Id;
        }).ConfigureAwait(false);

        return new CreateInstallmentPlanResponse(planId);
    }
}
