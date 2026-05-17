using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>
/// Month window aggregation (excludes <see cref="TransactionType.Transfer"/> and <see cref="TransactionType.Reversal"/>).
/// </summary>
internal static class MonthlyPeriodSummaryCalculator
{
    private static readonly JsonSerializerOptions JsonStoreOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    /// <summary>Expense types included in total expense / breakdowns.</summary>
    public static readonly TransactionType[] ExpenseAggregateTypes =
    {
        TransactionType.Direct,
        TransactionType.Deferred,
        TransactionType.Split,
        TransactionType.DebtBorrow,
        TransactionType.LoanGive,
    };

    /// <summary>Transactions included in monthly aggregates (not transfer/reversal; not soft-deleted).</summary>
    public static IQueryable<FinTransaction> MonthTransactions(
        IApplicationDbContext db,
        Guid smoduleId,
        int year,
        int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return db.FinTransactions
            .AsNoTracking()
            .Where(t => t.SmoduleId == smoduleId
                        && !t.IsDeleted
                        && t.Type != TransactionType.Reversal
                        && t.Type != TransactionType.Transfer
                        && t.TxnDate >= start
                        && t.TxnDate <= end);
    }

    private static bool IsExpenseAggregate(TransactionType t) =>
        t is TransactionType.Direct
            or TransactionType.Deferred
            or TransactionType.Split
            or TransactionType.DebtBorrow
            or TransactionType.LoanGive;

    /// <summary>Income, expense, net, category &amp; source expense breakdowns, percentages vs total expense.</summary>
    public static async Task<(decimal TotalIncome, decimal TotalExpense, decimal Net, IReadOnlyList<MonthCategoryBreakdownItemDto> Categories, IReadOnlyList<MonthSourceBreakdownItemDto> Sources)>
        ComputeBreakdownsAsync(
            IApplicationDbContext db,
            Guid smoduleId,
            int year,
            int month,
            CancellationToken cancellationToken)
    {
        var q = MonthTransactions(db, smoduleId, year, month);

        var income = await q
            .Where(t => t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var expense = await q
            .Where(t =>
                t.Type == TransactionType.Direct
                || t.Type == TransactionType.Deferred
                || t.Type == TransactionType.Split
                || t.Type == TransactionType.DebtBorrow
                || t.Type == TransactionType.LoanGive)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var net = income - expense;

        var catAgg = await q
            .Where(t =>
                t.Type == TransactionType.Direct
                || t.Type == TransactionType.Deferred
                || t.Type == TransactionType.Split
                || t.Type == TransactionType.DebtBorrow
                || t.Type == TransactionType.LoanGive)
            .GroupBy(t => t.CategoryId)
            .Select(g => new { CategoryId = g.Key, Amount = g.Sum(x => x.Amount), Cnt = g.Count() })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var catIds = catAgg.Select(c => c.CategoryId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var catNames = await db.FinCategories
            .AsNoTracking()
            .Where(c => catIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken)
            .ConfigureAwait(false);

        var categories = catAgg
            .OrderByDescending(x => x.Amount)
            .Select(x =>
            {
                var name = x.CategoryId is { } cid && catNames.TryGetValue(cid, out var n)
                    ? n
                    : "(Uncategorised)";
                var pct = expense > 0 ? decimal.Round(x.Amount / expense * 100m, 2, MidpointRounding.AwayFromZero) : 0m;
                return new MonthCategoryBreakdownItemDto(x.CategoryId, name, x.Amount, x.Cnt, pct);
            })
            .ToList();

        var srcAgg = await q
            .Where(t =>
                t.Type == TransactionType.Direct
                || t.Type == TransactionType.Deferred
                || t.Type == TransactionType.Split
                || t.Type == TransactionType.DebtBorrow
                || t.Type == TransactionType.LoanGive)
            .GroupBy(t => t.SourceId)
            .Select(g => new { SourceId = g.Key, Amount = g.Sum(x => x.Amount) })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var srcIds = srcAgg.Select(s => s.SourceId).Distinct().ToList();
        var srcNames = await db.FinSources
            .AsNoTracking()
            .Where(s => srcIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken)
            .ConfigureAwait(false);

        var sources = srcAgg
            .OrderByDescending(x => x.Amount)
            .Select(x => new MonthSourceBreakdownItemDto(
                x.SourceId,
                srcNames.GetValueOrDefault(x.SourceId, "(Source)"),
                x.Amount))
            .ToList();

        return (income, expense, net, categories, sources);
    }

    /// <summary>Serialises breakdown rows for <see cref="FinMonthlyPeriod"/> JSON columns.</summary>
    public static string SerialiseCategoryBreakdown(IReadOnlyList<MonthCategoryBreakdownItemDto> rows) =>
        JsonSerializer.Serialize(rows, JsonStoreOptions);

    public static string SerialiseSourceBreakdown(IReadOnlyList<MonthSourceBreakdownItemDto> rows) =>
        JsonSerializer.Serialize(rows, JsonStoreOptions);

    /// <summary>Top expense categories (legacy summary DTO shape).</summary>
    public static async Task<(decimal TotalIncome, decimal TotalExpense, IReadOnlyList<CategoryAmountBreakdownDto> TopExpenseCategories)>
        ComputeAsync(
            IApplicationDbContext db,
            Guid smoduleId,
            int year,
            int month,
            CancellationToken cancellationToken)
    {
        var (income, expense, _, categories, _) = await ComputeBreakdownsAsync(db, smoduleId, year, month, cancellationToken)
            .ConfigureAwait(false);

        var top = categories
            .Where(c => c.CategoryId.HasValue)
            .Take(5)
            .Select(c => new CategoryAmountBreakdownDto(c.CategoryId!.Value, c.CategoryName, c.Amount))
            .ToList();

        return (income, expense, top);
    }

    /// <summary>Full report payload including daily grid and MoM comparison.</summary>
    public static async Task<MonthlyReportDto> BuildReportAsync(
        IApplicationDbContext db,
        Guid smoduleId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var (income, expense, net, categories, sources) = await ComputeBreakdownsAsync(db, smoduleId, year, month, cancellationToken)
            .ConfigureAwait(false);

        decimal? savings = income > 0
            ? decimal.Round(net / income * 100m, 2, MidpointRounding.AwayFromZero)
            : null;

        var q = MonthTransactions(db, smoduleId, year, month);
        var txnRows = await q
            .Select(t => new { t.TxnDate, t.Type, t.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyList = new List<DailyCashflowDto>(daysInMonth);
        for (var d = 1; d <= daysInMonth; d++)
        {
            var date = new DateOnly(year, month, d);
            var incomeD = txnRows.Where(r => r.TxnDate == date && r.Type == TransactionType.Income).Sum(r => r.Amount);
            var expD = txnRows
                .Where(r => r.TxnDate == date && IsExpenseAggregate(r.Type))
                .Sum(r => r.Amount);
            dailyList.Add(new DailyCashflowDto(date, incomeD, expD));
        }

        var topTxns = await (
                from t in q
                join s in db.FinSources.AsNoTracking() on t.SourceId equals s.Id
                join c in db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id
                    into cj
                from c in cj.DefaultIfEmpty()
                orderby t.Amount descending
                select new MonthlyReportTopTransactionDto(
                    t.Id,
                    t.Type,
                    t.Amount,
                    t.Description,
                    t.TxnDate,
                    c != null ? c.Name : null,
                    s.Name))
            .Take(5)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var prev = PrevMonth(year, month);
        var (pIncome, pExpense, pNet, _, _) = await ComputeBreakdownsAsync(db, smoduleId, prev.Year, prev.Month, cancellationToken)
            .ConfigureAwait(false);

        static decimal? Pct(decimal cur, decimal prevVal) =>
            prevVal == 0 ? null : decimal.Round((cur - prevVal) / prevVal * 100m, 2, MidpointRounding.AwayFromZero);

        var comparison = new MonthOverMonthComparisonDto(
            Pct(income, pIncome),
            Pct(expense, pExpense),
            Pct(net, pNet));

        var summary = new MonthlyReportSummaryDto(income, expense, net, savings);
        return new MonthlyReportDto(summary, categories, sources, topTxns, dailyList, comparison);
    }

    private static (int Year, int Month) PrevMonth(int year, int month) =>
        month == 1 ? (year - 1, 12) : (year, month - 1);
}
