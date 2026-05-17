using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Utils;
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

        await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var plan = await _db.FinInstallmentPlans
            .Include(p => p.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            .ConfigureAwait(false);

        if (plan is null || plan.Status != InstallmentStatus.Active)
            throw new NotFoundException("Installment plan was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(plan.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to record payments for this plan.");

        if (_currentUser.CurrentOrgId is { } orgId && plan.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this plan.");

        var pay = await _db.FinInstallmentPays
            .FirstOrDefaultAsync(
                p => p.PlanId == request.PlanId && p.InstallmentNumber == request.InstallmentNumber,
                cancellationToken)
            .ConfigureAwait(false);

        if (pay is null)
            throw new NotFoundException("Installment pay row was not found.");

        if (pay.Status is not (InstallmentPayStatus.Due or InstallmentPayStatus.Overdue))
            throw new BusinessRuleException("The installment must be due or overdue to accept a payment.");

        var paymentSource = await _db.FinSources
            .FirstOrDefaultAsync(s => s.Id == request.PaymentSourceId && s.SmoduleId == plan.SmoduleId, cancellationToken)
            .ConfigureAwait(false);

        if (paymentSource is null || paymentSource.IsDeleted)
            throw new BusinessRuleException("Payment source was not found or is inactive.");

        if (paymentSource.IsArchived)
            throw new BusinessRuleException("The payment source is archived and cannot be used.");

        if (paymentSource.Balance < pay.Amount)
            throw new BusinessRuleException("Insufficient balance on the payment source.");

        var note = $"Trả góp kỳ {pay.InstallmentNumber}/{plan.TotalMonths}";
        var description = note.Length <= 512 ? note : note[..512];

        var txn = new FinTransaction
        {
            SmoduleId = plan.SmoduleId,
            Type = TransactionType.Direct,
            Status = TxnStatus.Completed,
            Amount = pay.Amount,
            Currency = paymentSource.Currency,
            TxnDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SourceId = paymentSource.Id,
            CategoryId = null,
            Description = description,
            Note = note.Length <= 500 ? note : note[..500],
            InstallmentPlanId = plan.Id,
        };

        _db.FinTransactions.Add(txn);

        paymentSource.Balance -= pay.Amount;

        pay.PaidAmount = pay.Amount;
        pay.Status = InstallmentPayStatus.Paid;
        pay.PaidAt = DateTime.UtcNow;
        pay.TxnId = txn.Id;

        var allPaid = await _db.FinInstallmentPays
            .Where(p => p.PlanId == plan.Id)
            .AllAsync(p => p.Status == InstallmentPayStatus.Paid, cancellationToken)
            .ConfigureAwait(false);

        if (allPaid)
            plan.Status = InstallmentStatus.Completed;

        FinTransactionHistoryHelper.AddCreated(_db, _currentUser, txn);

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

        return new RecordInstallmentPaymentResponse(txn.Id);
    }
}
