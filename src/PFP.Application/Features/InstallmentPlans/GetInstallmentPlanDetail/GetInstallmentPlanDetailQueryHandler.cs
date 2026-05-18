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
            .Include(p => p.Pays)
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken)
            .ConfigureAwait(false);

        if (plan is null)
            throw new NotFoundException("Installment plan was not found.");

        if (!await _currentUser
                .HasSpaceModuleAccessAsync(plan.SmoduleId, SpaceRole.Viewer, cancellationToken)
                .ConfigureAwait(false))
            throw new UnauthorizedAppException("You do not have permission to view this installment plan.");

        var pays = plan.Pays
            .OrderBy(p => p.InstallmentNumber)
            .Select(p => new InstallmentPayItemDto(
                p.InstallmentNumber,
                p.DueDate,
                CurrencyUnits.ToWhole(p.Amount),
                CurrencyUnits.ToWhole(p.PaidAmount),
                p.Status,
                p.PaidAt,
                p.TxnId))
            .ToList();

        var dto = new InstallmentPlanDetailDto(
            plan.Id,
            plan.SmoduleId,
            plan.SourceId,
            plan.Source.Name,
            plan.OriginalTxnId,
            plan.OriginalTransaction.Description,
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
            pays);

        return new GetInstallmentPlanDetailResponse(dto);
    }
}
