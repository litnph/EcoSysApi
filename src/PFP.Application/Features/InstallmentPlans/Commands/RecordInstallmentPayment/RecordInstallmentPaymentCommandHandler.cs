using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Utils;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Commands.RecordInstallmentPayment;

/// <summary>Posts a direct payment for one installment line and may complete the parent plan.</summary>
public sealed class RecordInstallmentPaymentCommandHandler : IRequestHandler<RecordInstallmentPaymentCommand, RecordInstallmentPaymentResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public RecordInstallmentPaymentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<RecordInstallmentPaymentResponse> Handle(
        RecordInstallmentPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var plan = await _db.FinInstallmentPlans
                .FirstOrDefaultAsync(p => p.Id == request.PlanId, ct)
                .ConfigureAwait(false);

            if (plan is null || plan.Status != InstallmentStatus.Active)
                throw new NotFoundException("Installment plan was not found.");

            OptimisticConcurrencyGuard.Ensure(plan.Version, request.ExpectedVersion);
            var pay = await _db.FinInstallmentPays
                .FirstOrDefaultAsync(
                    p => p.PlanId == request.PlanId && p.InstallmentNumber == request.InstallmentNumber,
                    ct)
                .ConfigureAwait(false);

            if (pay is null)
                throw new NotFoundException("Installment pay row was not found.");

            var today = FinanceBusinessCalendar.Today;
            var effectiveStatus = InstallmentPaySchedule.ResolveStatus(pay, today);
            if (effectiveStatus == InstallmentPayStatus.Paid)
                throw new BusinessRuleException("The installment has already been paid.");

        var paymentSource = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.PaymentSourceId, cancellationToken)
            .ConfigureAwait(false);

        if (paymentSource is null || paymentSource.IsDeleted)
            throw new BusinessRuleException("Payment source was not found or is inactive.");

        if (paymentSource.IsArchived)
            throw new BusinessRuleException("The payment source is archived and cannot be used.");

        if (paymentSource.Type == SourceType.CreditCard)
            throw new BusinessRuleException("Installment payments require a non-credit-card source.");

        var planSource = await _db.FinSources
            .FirstAsync(s => s.Id == plan.SourceId, cancellationToken)
            .ConfigureAwait(false);
        if (!string.Equals(paymentSource.Currency, planSource.Currency, StringComparison.Ordinal))
            throw new BusinessRuleException("Payment source currency must match the installment plan currency.");

        var capturedByCycle = await _db.FinBillingCycles
            .AsNoTracking()
            .AnyAsync(c => c.SourceId == plan.SourceId
                           && c.Status != BillingCycleStatus.Paid
                           && c.StatementDate == pay.StatementDate,
                cancellationToken)
            .ConfigureAwait(false);
        if (capturedByCycle)
            throw new BusinessRuleException("This installment is captured by a billing cycle and must be paid through that cycle.");

        if (paymentSource.Balance < pay.Amount)
            throw new BusinessRuleException("Insufficient balance on the payment source.");

        if (planSource.Balance < pay.Amount)
            throw new BusinessRuleException("Credit-card outstanding balance is lower than the installment amount.");

        var note = $"Trả góp kỳ {pay.InstallmentNumber}/{plan.TotalMonths}";
        var description = note.Length <= 512 ? note : note[..512];

        var txn = new FinTransaction
        {
Type = TransactionType.Direct,
            Purpose = TransactionPurpose.InstallmentPayment,
            Status = TxnStatus.New,
            Amount = pay.Amount,
            Currency = paymentSource.Currency,
            TxnDate = FinanceBusinessCalendar.Today,
            SourceId = paymentSource.Id,
            CategoryId = null,
            Description = description,
            Note = note.Length <= 500 ? note : note[..500],
            InstallmentPlanId = plan.Id,
        };

        _db.FinTransactions.Add(txn);

        paymentSource.Balance -= pay.Amount;
        planSource.Balance -= pay.Amount;

        pay.PaidAmount = pay.Amount;
        pay.Status = InstallmentPayStatus.Paid;
        pay.PaidAt = DateTime.UtcNow;
        pay.TxnId = txn.Id;

        var hasOtherUnpaid = await _db.FinInstallmentPays
            .Where(p => p.PlanId == plan.Id)
            .AnyAsync(p => p.Id != pay.Id && p.Status != InstallmentPayStatus.Paid, cancellationToken)
            .ConfigureAwait(false);

        if (!hasOtherUnpaid)
            plan.Status = InstallmentStatus.Completed;

        FinTransactionHistoryHelper.AddCreated(_db, _currentUser, txn);

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return new RecordInstallmentPaymentResponse(txn.Id);
        }, cancellationToken).ConfigureAwait(false);
    }
}
