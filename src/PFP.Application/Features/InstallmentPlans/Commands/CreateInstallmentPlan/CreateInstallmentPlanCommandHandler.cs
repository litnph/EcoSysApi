using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
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
            .AsNoTracking()
            .Include(t => t.Source)
            .ThenInclude(s => s.Smodule)
            .ThenInclude(m => m.Space)
            .FirstOrDefaultAsync(t => t.Id == request.OriginalTxnId, cancellationToken)
            .ConfigureAwait(false);

        if (txn is null || txn.IsDeleted || txn.Type != TransactionType.Deferred)
            throw new NotFoundException("Transaction was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(txn.SmoduleId, SpaceRole.Editor, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to create installment plans for this module.");

        if (_currentUser.CurrentOrgId is { } orgId && txn.Source.Smodule.Space.OrgId != orgId)
            throw new UnauthorizedAppException("The current organisation does not own this transaction.");

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
        var monthlyShare = decimal.Truncate(totalAmount / totalMonths);
        var lastShare = totalAmount - monthlyShare * (totalMonths - 1);
        if (lastShare < 0)
            throw new BusinessRuleException("Invalid installment amount split.");

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Wrap explicit transaction with the retrying execution strategy so EnableRetryOnFailure
        // can retry transient transport faults around the plan + schedule write.
        var strategy = _db.Database.CreateExecutionStrategy();
        var planId = await strategy.ExecuteAsync(async () =>
        {
            await using var dbTx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var plan = new FinInstallmentPlan
            {
                SmoduleId = txn.SmoduleId,
                OriginalTxnId = txn.Id,
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

            for (var i = 1; i <= totalMonths; i++)
            {
                var amount = i == totalMonths ? lastShare : monthlyShare;
                var dueDate = startDate.AddMonths(i - 1);
                var status = i == 1 && dueDate <= today ? InstallmentPayStatus.Due : InstallmentPayStatus.Upcoming;
                _db.FinInstallmentPays.Add(
                    new FinInstallmentPay
                    {
                        PlanId = plan.Id,
                        InstallmentNumber = i,
                        DueDate = dueDate,
                        Amount = amount,
                        PaidAmount = 0,
                        Status = status,
                    });
            }

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await dbTx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return plan.Id;
        }).ConfigureAwait(false);

        return new CreateInstallmentPlanResponse(planId);
    }
}
