using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Rules for installment pay lines that belong on a billing-cycle statement.</summary>
public static class BillingCycleInstallmentRules
{
    /// <summary>
    /// Installment pays due in the statement month on the same card (excludes cancelled plans).
    /// </summary>
    public static bool IsPayDueInStatementMonth(FinInstallmentPay pay, DateOnly statementDate) =>
        pay.DueDate.Year == statementDate.Year && pay.DueDate.Month == statementDate.Month;

    public static IQueryable<FinInstallmentPay> DuePaysQuery(
        IApplicationDbContext db,
        Guid sourceId,
        DateOnly statementDate) =>
        from pay in db.FinInstallmentPays.AsNoTracking()
        join plan in db.FinInstallmentPlans.AsNoTracking() on pay.PlanId equals plan.Id
        where plan.SourceId == sourceId
              && plan.Status != InstallmentStatus.Cancelled
              && pay.DueDate.Year == statementDate.Year
              && pay.DueDate.Month == statementDate.Month
        select pay;

    public static async Task<IReadOnlyList<FinBillingCycleInstallmentDueDto>> LoadDueDtosAsync(
        IApplicationDbContext db,
        FinBillingCycle cycle,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from pay in db.FinInstallmentPays.AsNoTracking()
            join plan in db.FinInstallmentPlans.AsNoTracking() on pay.PlanId equals plan.Id
            join txn in db.FinTransactions.AsNoTracking() on plan.OriginalTxnId equals txn.Id
            join cat in db.FinCategories.AsNoTracking() on txn.CategoryId equals cat.Id into catJoin
            from cat in catJoin.DefaultIfEmpty()
            where plan.SourceId == cycle.SourceId
                  && plan.Status != InstallmentStatus.Cancelled
                  && pay.DueDate.Year == cycle.StatementDate.Year
                  && pay.DueDate.Month == cycle.StatementDate.Month
            orderby pay.DueDate, pay.InstallmentNumber
            select new
            {
                Pay = pay,
                plan.Id,
                plan.OriginalTxnId,
                plan.TotalMonths,
                txn.Description,
                CategoryName = cat != null ? cat.Name : null,
            }
        ).ToListAsync(cancellationToken).ConfigureAwait(false);

        return rows
            .Select(r => FinBillingCycleDtoMapper.ToInstallmentDueDto(
                r.Pay,
                r.OriginalTxnId,
                r.TotalMonths,
                r.Description,
                r.CategoryName))
            .ToList();
    }
}
