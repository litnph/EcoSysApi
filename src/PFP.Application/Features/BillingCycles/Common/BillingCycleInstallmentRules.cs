using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Rules for installment pay lines that belong on a billing-cycle statement.</summary>
public static class BillingCycleInstallmentRules
{
    /// <summary>
    /// Installment pays captured on the exact statement date for the same card.
    /// </summary>
    public static bool IsPayDueOnStatement(FinInstallmentPay pay, DateOnly statementDate) =>
        pay.StatementDate == statementDate;

    public static IQueryable<FinInstallmentPay> DuePaysQuery(
        IApplicationDbContext db,
        Guid sourceId,
        DateOnly statementDate) =>
        from pay in db.FinInstallmentPays.AsNoTracking()
        join plan in db.FinInstallmentPlans.AsNoTracking() on pay.PlanId equals plan.Id
        where plan.SourceId == sourceId
              && plan.Status != InstallmentStatus.Cancelled
              && pay.Status != InstallmentPayStatus.Paid
              && pay.StatementDate == statementDate
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
                  && pay.Status != InstallmentPayStatus.Paid
                  && pay.StatementDate == cycle.StatementDate
            orderby pay.DueDate, pay.InstallmentNumber
            select new
            {
                Pay = pay,
                plan.Id,
                plan.OriginalTxnId,
                plan.TotalMonths,
                txn.Description,
                txn.CategoryId,
                CategoryName = cat != null ? cat.Name : null,
            }
        ).ToListAsync(cancellationToken).ConfigureAwait(false);

        return rows
            .Select(r => FinBillingCycleDtoMapper.ToInstallmentDueDto(
                r.Pay,
                r.OriginalTxnId,
                r.TotalMonths,
                r.Description,
                r.CategoryName,
                r.CategoryId))
            .ToList();
    }

    /// <summary>
    /// Loads installment dues for several cycles in one database query, keyed by billing-cycle id.
    /// </summary>
    public static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<FinBillingCycleInstallmentDueDto>>>
        LoadDueDtosByCycleAsync(
            IApplicationDbContext db,
            IReadOnlyCollection<FinBillingCycle> cycles,
            CancellationToken cancellationToken)
    {
        if (cycles.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<FinBillingCycleInstallmentDueDto>>();

        var sourceIds = cycles.Select(cycle => cycle.SourceId).Distinct().ToList();
        var firstStatement = cycles.Min(cycle => cycle.StatementDate);
        var lastStatement = cycles.Max(cycle => cycle.StatementDate);
        var rangeStart = new DateOnly(firstStatement.Year, firstStatement.Month, 1);
        var rangeEnd = new DateOnly(
            lastStatement.Year,
            lastStatement.Month,
            DateTime.DaysInMonth(lastStatement.Year, lastStatement.Month));

        var rows = await (
            from pay in db.FinInstallmentPays.AsNoTracking()
            join plan in db.FinInstallmentPlans.AsNoTracking() on pay.PlanId equals plan.Id
            join txn in db.FinTransactions.AsNoTracking() on plan.OriginalTxnId equals txn.Id
            join cat in db.FinCategories.AsNoTracking() on txn.CategoryId equals cat.Id into catJoin
            from cat in catJoin.DefaultIfEmpty()
            where sourceIds.Contains(plan.SourceId)
                  && plan.Status != InstallmentStatus.Cancelled
                  && pay.Status != InstallmentPayStatus.Paid
                  && pay.StatementDate >= rangeStart
                  && pay.StatementDate <= rangeEnd
            select new
            {
                Pay = pay,
                SourceId = plan.SourceId,
                plan.Id,
                plan.OriginalTxnId,
                plan.TotalMonths,
                txn.Description,
                txn.CategoryId,
                CategoryName = cat != null ? cat.Name : null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return cycles.ToDictionary(
            cycle => cycle.Id,
            cycle => (IReadOnlyList<FinBillingCycleInstallmentDueDto>)rows
                .Where(row => row.SourceId == cycle.SourceId
                              && IsPayDueOnStatement(row.Pay, cycle.StatementDate))
                .OrderBy(row => row.Pay.DueDate)
                .ThenBy(row => row.Pay.InstallmentNumber)
                .Select(row => FinBillingCycleDtoMapper.ToInstallmentDueDto(
                    row.Pay,
                    row.OriginalTxnId,
                    row.TotalMonths,
                    row.Description,
                    row.CategoryName,
                    row.CategoryId))
                .ToList());
    }
}
