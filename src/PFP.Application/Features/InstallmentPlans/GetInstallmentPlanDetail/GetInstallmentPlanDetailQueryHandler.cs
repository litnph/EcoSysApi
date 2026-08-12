using MediatR;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentPlanDetail;

/// <summary>Handles <see cref="GetInstallmentPlanDetailQuery"/>.</summary>
public sealed class GetInstallmentPlanDetailQueryHandler : IRequestHandler<GetInstallmentPlanDetailQuery, GetInstallmentPlanDetailResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    /// <summary>Creates the handler.</summary>
    public GetInstallmentPlanDetailQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task<GetInstallmentPlanDetailResponse> Handle(
        GetInstallmentPlanDetailQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            throw new UnauthorizedAppException("Authentication is required.");

        var plan = await _db.FinInstallmentPlans
            .AsNoTracking()
            .Include(p => p.Source)
            .Include(p => p.OriginalTransaction)
                .ThenInclude(t => t.Category)
            .Include(p => p.Pays)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            .ConfigureAwait(false);

        if (plan is null)
            throw new NotFoundException("Installment plan was not found.");

        var today = FinanceBusinessCalendar.Today;
        var capturedStatementDateRows = await _db.FinBillingCycles
            .AsNoTracking()
            .Where(c => c.SourceId == plan.SourceId && c.Status != BillingCycleStatus.Paid)
            .Select(c => c.StatementDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var capturedStatementDates = capturedStatementDateRows.ToHashSet();
        var pays = plan.Pays
            .OrderBy(p => p.InstallmentNumber)
            .Select(p => new InstallmentPayItemDto(
                p.InstallmentNumber,
                p.StatementDate,
                p.DueDate,
                CurrencyUnits.ToWhole(p.Amount),
                CurrencyUnits.ToWhole(p.PaidAmount),
                InstallmentPaySchedule.ResolveStatus(p, today),
                p.PaidAt,
                p.TxnId,
                p.Status != InstallmentPayStatus.Paid
                    && !capturedStatementDates.Contains(p.StatementDate)))
            .ToList();

        var dto = new InstallmentPlanDetailDto(
            plan.Id,
            plan.SourceId,
            plan.Source.Name,
            plan.Source.Icon,
            plan.Source.Color,
            plan.OriginalTxnId,
            plan.OriginalTransaction.Description,
            plan.OriginalTransaction.Category?.Name,
            CurrencyUnits.ToWhole(plan.TotalAmount),
            plan.TotalMonths,
            CurrencyUnits.ToWhole(plan.MonthlyAmount),
            plan.InterestRate,
            plan.ConversionFeeRate,
            plan.ConversionFeeAmount is { } fee ? CurrencyUnits.ToWhole(fee) : null,
            plan.ConversionFeeStatus,
            plan.ConversionFeeTxnId,
            plan.StartDate,
            plan.Status,
            plan.CancellationReason,
            InstallmentPlanRules.CanDelete(plan),
            plan.Version,
            pays);

        return new GetInstallmentPlanDetailResponse(dto);
    }
}
