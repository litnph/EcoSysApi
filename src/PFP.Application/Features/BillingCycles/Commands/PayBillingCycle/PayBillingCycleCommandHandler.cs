using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.Transactions.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;

/// <summary>Records a direct payment, moves balances on both sources, and updates cycle paid totals.</summary>
public sealed class PayBillingCycleCommandHandler : IRequestHandler<PayBillingCycleCommand, PayBillingCycleResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public PayBillingCycleCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<PayBillingCycleResponse> Handle(PayBillingCycleCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        return await DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var cycle = await _db.FinBillingCycles
            .Include(bc => bc.Source)
            .FirstOrDefaultAsync(bc => bc.Id == request.CycleId, cancellationToken)
            .ConfigureAwait(false);

        if (cycle is null)
            throw new NotFoundException("Billing cycle was not found.");

        if (cycle.Status is not (BillingCycleStatus.Closed or BillingCycleStatus.Overdue))
            throw new BusinessRuleException("Billing cycle must be closed or overdue to accept a payment.");
if (request.PaymentSourceId == cycle.SourceId)
            throw new BusinessRuleException("Payment source cannot be the same as the credit card source for this cycle.");

        var paymentSource = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.PaymentSourceId, cancellationToken)
            .ConfigureAwait(false);

        if (paymentSource is null || paymentSource.IsDeleted)
            throw new BusinessRuleException("Payment source was not found or is inactive.");

        if (paymentSource.IsArchived)
            throw new BusinessRuleException("The payment source is archived and cannot be used.");

        if (paymentSource.Type == SourceType.CreditCard)
            throw new BusinessRuleException("Billing-cycle payments require a non-credit-card source.");

        if (!string.Equals(paymentSource.Currency, cycle.Source.Currency, StringComparison.Ordinal))
            throw new BusinessRuleException("Payment source currency must match the credit card currency.");

        var paymentAmount = CurrencyUnits.FromWhole(request.Amount);
        var remaining = cycle.TotalAmount - cycle.PaidAmount;
        if (paymentAmount > remaining)
            throw new BusinessRuleException("Amount cannot exceed the remaining balance for this cycle.");

        if (paymentSource.Balance < paymentAmount)
            throw new BusinessRuleException("Insufficient balance on the payment source.");

        var cardSource = cycle.Source;

        var note = BillingCyclePaymentNotes.StatementPayment;
        var description = note.Length <= 512 ? note : note[..512];

        var payTxn = new FinTransaction
        {
Type = TransactionType.Direct,
            Purpose = TransactionPurpose.StatementPayment,
            Status = TxnStatus.New,
            Amount = paymentAmount,
            Currency = paymentSource.Currency,
            TxnDate = FinanceBusinessCalendar.Today,
            SourceId = paymentSource.Id,
            CategoryId = null,
            Description = description,
            Note = note,
            BillingCycleId = cycle.Id,
        };

        _db.FinTransactions.Add(payTxn);

        paymentSource.Balance -= paymentAmount;
        cardSource.Balance -= paymentAmount;

        cycle.PaidAmount += paymentAmount;
        if (cycle.PaidAmount >= cycle.TotalAmount)
        {
            cycle.Status = BillingCycleStatus.Paid;
            cycle.PaidAt = DateTime.UtcNow;

            var installmentPays = await _db.FinInstallmentPays
                .Include(p => p.Plan)
                .Where(p => p.Plan.SourceId == cycle.SourceId
                            && p.Plan.Status != InstallmentStatus.Cancelled
                            && p.Status != InstallmentPayStatus.Paid
                            && p.StatementDate == cycle.StatementDate)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            foreach (var installmentPay in installmentPays)
            {
                installmentPay.Status = InstallmentPayStatus.Paid;
                installmentPay.PaidAmount = installmentPay.Amount;
                installmentPay.PaidAt = DateTime.UtcNow;
                installmentPay.TxnId = payTxn.Id;
            }

            foreach (var plan in installmentPays.Select(p => p.Plan).DistinctBy(p => p.Id))
            {
                var hasOtherUnpaid = await _db.FinInstallmentPays
                    .AnyAsync(p => p.PlanId == plan.Id
                                   && !installmentPays.Select(x => x.Id).Contains(p.Id)
                                   && p.Status != InstallmentPayStatus.Paid, ct)
                    .ConfigureAwait(false);
                if (!hasOtherUnpaid)
                    plan.Status = InstallmentStatus.Completed;
            }
        }

        var utcNow = DateTime.UtcNow;
        _db.FinTransactionHistory.Add(new FinTransactionHistory
        {
            TransactionId = payTxn.Id,
            Version = 1,
            ChangedBy = _currentUser.UserId,
            SessionId = _currentUser.SessionId,
            ChangeType = HistoryChangeType.Created,
            ChangedFields = null,
            Snapshot = TransactionHistoryJson.BuildCreatedSnapshot(payTxn),
            ChangeReason = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        });

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            return new PayBillingCycleResponse(FinBillingCycleDtoMapper.ToDto(cycle, cardSource.Name), payTxn.Id);
        }, cancellationToken).ConfigureAwait(false);
    }
}
