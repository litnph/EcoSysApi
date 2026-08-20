using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>
/// Month window aggregation (excludes <see cref="TransactionType.Transfer"/> and <see cref="TransactionType.Reversal"/>).
/// </summary>
internal static class MonthlyPeriodSummaryCalculator
{
    private const string FormulaVersion = "monthly-report-v2";
    private const string MetricBasis = "income/direct by transaction date; card spend/installments by statement date";

    private static readonly JsonSerializerOptions JsonStoreOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    /// <summary>Transactions in the calendar month (not soft-deleted).</summary>
    public static IQueryable<FinTransaction> MonthTransactions(
        IApplicationDbContext db,
        int year,
        int month)
    {
        var start = new DateOnly(year, month, 1);
        var end = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return db.FinTransactions
            .AsNoTracking()
            .Where(t => t.TxnDate >= start && t.TxnDate <= end);
    }

    /// <summary>
    /// Direct payment toward a billing cycle — expense is captured in the billing-cycle section, not direct spend.
    /// </summary>
    public static IQueryable<FinTransaction> ExcludeBillingCyclePaymentTxns(IQueryable<FinTransaction> q) =>
        q.Where(t =>
            t.Purpose != TransactionPurpose.StatementPayment
            && (t.Note != BillingCyclePaymentNotes.StatementPayment
                && t.Description != BillingCyclePaymentNotes.StatementPayment));

    private static IQueryable<FinTransaction> ReportExpenseTransactions(
        IApplicationDbContext db,
        int year,
        int month) =>
        ExcludeBillingCyclePaymentTxns(MonthTransactions(db, year, month))
            .Where(t =>
                t.Type == TransactionType.Deferred
                || t.Type == TransactionType.Split
                || t.Type == TransactionType.Direct);

    /// <summary>Income, expense, net, category &amp; source expense breakdowns, percentages vs total expense.</summary>
    public static async Task<(decimal TotalIncome, decimal TotalExpense, decimal Net, IReadOnlyList<MonthCategoryBreakdownItemDto> Categories, IReadOnlyList<MonthSourceBreakdownItemDto> Sources)>
        ComputeBreakdownsAsync(
            IApplicationDbContext db,
            int year,
            int month,
            CancellationToken cancellationToken)
    {
        var currencies = await GetReportCurrenciesAsync(db, year, month, cancellationToken)
            .ConfigureAwait(false);
        var currency = SelectPrimaryCurrency(currencies);
        var q = ReportExpenseTransactions(db, year, month)
            .Where(t => t.Currency == currency);

        var income = await MonthTransactions(db, year, month)
            .Where(t => t.Type == TransactionType.Income && t.Currency == currency)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var expense = await q
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

        var net = income - expense;

        var catAgg = await q
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
                return new MonthCategoryBreakdownItemDto(x.CategoryId, name, CurrencyUnits.ToWhole(x.Amount), x.Cnt, pct);
            })
            .ToList();

        var srcAgg = await q
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
                CurrencyUnits.ToWhole(x.Amount)))
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
            int year,
            int month,
            CancellationToken cancellationToken)
    {
        var (income, expense, _, categories, _) = await ComputeBreakdownsAsync(db, year, month, cancellationToken)
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
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var currencies = await GetReportCurrenciesAsync(db, year, month, cancellationToken)
            .ConfigureAwait(false);
        if (currencies.Count == 0)
            currencies = ["VND"];

        var groups = new List<MonthlyReportCurrencyGroupDto>(currencies.Count);
        foreach (var currency in currencies)
        {
            groups.Add(await BuildCurrencyGroupAsync(
                    db, year, month, currency, cancellationToken)
                .ConfigureAwait(false));
        }

        var primaryCurrency = SelectPrimaryCurrency(currencies);
        var primary = groups.Single(group => group.Currency == primaryCurrency);

        return new MonthlyReportDto(
            primary.Summary,
            primary.CategoryBreakdown,
            primary.SourceBreakdown,
            primary.TopTransactions,
            primary.DailyBreakdown,
            primary.ComparisonWithPreviousMonth,
            primary.DirectExpenses,
            primary.BillingCycles,
            new MonthlyReportMetadataDto(
                FormulaVersion,
                MetricBasis,
                primaryCurrency,
                FinanceBusinessCalendar.TimeZoneId,
                ConsolidatedTotalsAvailable: groups.Count == 1),
            groups);
    }

    private static async Task<MonthlyReportCurrencyGroupDto> BuildCurrencyGroupAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        CancellationToken cancellationToken)
    {
        var directExpenses = await BuildDirectExpensesSectionAsync(
                db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);
        var billingCycles = await BuildBillingCyclesSectionAsync(
                db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);

        var income = await SumMonthIncomeAsync(db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);
        var reportExpenseLong = directExpenses.TotalAmount + billingCycles.TotalAmount;
        var reportExpense = CurrencyUnits.FromWhole(reportExpenseLong);
        var net = income - reportExpense;

        decimal? savings = income > 0
            ? decimal.Round(net / income * 100m, 2, MidpointRounding.AwayFromZero)
            : null;

        var categories = BuildReportCategories(directExpenses, billingCycles, reportExpenseLong);
        var sources = BuildReportSources(directExpenses, billingCycles);
        var topTxns = BuildReportTopTransactions(directExpenses, billingCycles);

        var prev = PrevMonth(year, month);
        var previousCurrencies = await GetReportCurrenciesAsync(
                db, prev.Year, prev.Month, cancellationToken)
            .ConfigureAwait(false);
        var comparison = previousCurrencies.Contains(currency, StringComparer.Ordinal)
            ? await BuildPreviousMonthComparisonAsync(
                    db, prev.Year, prev.Month, currency, income, reportExpense, net, cancellationToken)
                .ConfigureAwait(false)
            : new MonthOverMonthComparisonDto(null, null, null);

        var dailyListResolved = await BuildReportDailyBreakdownAsync(
                db, year, month, currency, directExpenses, billingCycles, cancellationToken)
            .ConfigureAwait(false);

        var summary = new MonthlyReportSummaryDto(
            CurrencyUnits.ToWhole(income),
            reportExpenseLong,
            CurrencyUnits.ToWhole(net),
            savings);

        return new MonthlyReportCurrencyGroupDto(
            currency,
            summary,
            categories,
            sources,
            topTxns,
            dailyListResolved,
            comparison,
            directExpenses,
            billingCycles);
    }

    private static async Task<IReadOnlyList<string>> GetReportCurrenciesAsync(
        IApplicationDbContext db,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var transactionCurrencies = await MonthTransactions(db, year, month)
            .Where(t => t.Type == TransactionType.Income
                        || t.Type == TransactionType.Direct
                        || t.Type == TransactionType.Deferred
                        || t.Type == TransactionType.Split)
            .Select(t => t.Currency)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var statementCurrencies = await db.FinBillingCycles
            .AsNoTracking()
            .Where(c => c.StatementDate.Year == year && c.StatementDate.Month == month)
            .Select(c => c.Source.Currency)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return transactionCurrencies
            .Concat(statementCurrencies)
            .Where(currency => !string.IsNullOrWhiteSpace(currency))
            .Select(currency => currency.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(currency => currency, StringComparer.Ordinal)
            .ToList();
    }

    private static string SelectPrimaryCurrency(IReadOnlyList<string> currencies) =>
        currencies.Contains("VND", StringComparer.Ordinal)
            ? "VND"
            : currencies.FirstOrDefault() ?? "VND";

    private static async Task<MonthOverMonthComparisonDto> BuildPreviousMonthComparisonAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        decimal currentIncome,
        decimal currentExpense,
        decimal currentNet,
        CancellationToken cancellationToken)
    {
        var previousIncome = await SumMonthIncomeAsync(db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);
        var previousDirect = await BuildDirectExpensesSectionAsync(
                db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);
        var previousBilling = await BuildBillingCyclesSectionAsync(
                db, year, month, currency, cancellationToken)
            .ConfigureAwait(false);
        var previousExpense = CurrencyUnits.FromWhole(
            previousDirect.TotalAmount + previousBilling.TotalAmount);
        var previousNet = previousIncome - previousExpense;

        static decimal? Percent(decimal current, decimal previous) =>
            previous == 0
                ? null
                : decimal.Round(
                    (current - previous) / previous * 100m,
                    2,
                    MidpointRounding.AwayFromZero);

        return new MonthOverMonthComparisonDto(
            Percent(currentIncome, previousIncome),
            Percent(currentExpense, previousExpense),
            Percent(currentNet, previousNet));
    }

    private static async Task<decimal> SumMonthIncomeAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        CancellationToken cancellationToken) =>
        await MonthTransactions(db, year, month)
            .Where(t => t.Type == TransactionType.Income && t.Currency == currency)
            .SumAsync(t => (decimal?)t.Amount, cancellationToken)
            .ConfigureAwait(false) ?? 0m;

    private static IReadOnlyList<MonthCategoryBreakdownItemDto> BuildReportCategories(
        MonthlyReportDirectExpenseSectionDto direct,
        MonthlyReportBillingCyclesSectionDto billing,
        long totalExpense)
    {
        var map = new Dictionary<string, (Guid? CategoryId, string Name, long Amount, int Count)>(
            StringComparer.Ordinal);

        void Add(Guid? categoryId, string? categoryName, long amount)
        {
            var name = string.IsNullOrWhiteSpace(categoryName) ? "(Uncategorised)" : categoryName.Trim();
            var key = categoryId?.ToString("N") ?? "uncategorised";
            if (!map.TryGetValue(key, out var cur))
                cur = (categoryId, name, 0, 0);
            map[key] = (cur.CategoryId, cur.Name, cur.Amount + amount, cur.Count + 1);
        }

        foreach (var item in direct.Items)
            Add(item.CategoryId, item.CategoryName, item.Amount);

        foreach (var cycle in billing.Cycles)
        {
            foreach (var txn in cycle.Transactions)
                Add(txn.CategoryId, txn.CategoryName, txn.Amount);
            foreach (var due in cycle.InstallmentDues)
                Add(due.CategoryId, due.CategoryName, due.Amount);
        }

        return map
            .OrderByDescending(x => x.Value.Amount)
            .Select(x =>
            {
                var pct = totalExpense > 0
                    ? decimal.Round(x.Value.Amount / (decimal)totalExpense * 100m, 2, MidpointRounding.AwayFromZero)
                    : 0m;
                return new MonthCategoryBreakdownItemDto(
                    x.Value.CategoryId,
                    x.Value.Name,
                    x.Value.Amount,
                    x.Value.Count,
                    pct);
            })
            .ToList();
    }

    private static IReadOnlyList<MonthSourceBreakdownItemDto> BuildReportSources(
        MonthlyReportDirectExpenseSectionDto direct,
        MonthlyReportBillingCyclesSectionDto billing)
    {
        var map = new Dictionary<Guid, (string Name, long Amount)>();

        foreach (var item in direct.Items)
        {
            var name = string.IsNullOrWhiteSpace(item.SourceName) ? "(Source)" : item.SourceName.Trim();
            var current = map.GetValueOrDefault(item.SourceId);
            map[item.SourceId] = (name, current.Amount + item.Amount);
        }

        foreach (var cycle in billing.Cycles)
        {
            var name = string.IsNullOrWhiteSpace(cycle.SourceName) ? "(Source)" : cycle.SourceName.Trim();
            var current = map.GetValueOrDefault(cycle.SourceId);
            map[cycle.SourceId] = (name, current.Amount + cycle.TotalAmount);
        }

        return map
            .OrderByDescending(x => x.Value.Amount)
            .Select(x => new MonthSourceBreakdownItemDto(x.Key, x.Value.Name, x.Value.Amount))
            .ToList();
    }

    private static async Task<IReadOnlyList<DailyCashflowDto>> BuildReportDailyBreakdownAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        MonthlyReportDirectExpenseSectionDto direct,
        MonthlyReportBillingCyclesSectionDto billing,
        CancellationToken cancellationToken)
    {
        var q = MonthTransactions(db, year, month);
        var incomeRows = await q
            .Where(t => t.Type == TransactionType.Income && t.Currency == currency)
            .Select(t => new { t.TxnDate, t.Amount })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var directByDay = direct.Items
            .GroupBy(i => i.TxnDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var statementsByDay = billing.Cycles
            .GroupBy(c => c.StatementDate)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalAmount));

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyList = new List<DailyCashflowDto>(daysInMonth);
        for (var d = 1; d <= daysInMonth; d++)
        {
            var date = new DateOnly(year, month, d);
            var incomeD = incomeRows.Where(r => r.TxnDate == date).Sum(r => r.Amount);
            var expD = directByDay.GetValueOrDefault(date) + statementsByDay.GetValueOrDefault(date);
            dailyList.Add(new DailyCashflowDto(
                date,
                CurrencyUnits.ToWhole(incomeD),
                expD));
        }

        return dailyList;
    }

    private static IReadOnlyList<MonthlyReportTopTransactionDto> BuildReportTopTransactions(
        MonthlyReportDirectExpenseSectionDto direct,
        MonthlyReportBillingCyclesSectionDto billing)
    {
        var rows = new List<MonthlyReportTopTransactionDto>();

        foreach (var item in direct.Items)
        {
            rows.Add(new MonthlyReportTopTransactionDto(
                item.Id,
                TransactionType.Direct,
                item.Amount,
                item.Description,
                item.TxnDate,
                item.CategoryName,
                item.SourceName));
        }

        foreach (var cycle in billing.Cycles)
        {
            foreach (var txn in cycle.Transactions)
            {
                rows.Add(new MonthlyReportTopTransactionDto(
                    txn.Id,
                    TransactionType.Deferred,
                    txn.Amount,
                    txn.Description,
                    txn.TxnDate,
                    txn.CategoryName,
                    cycle.SourceName));
            }

            foreach (var due in cycle.InstallmentDues)
            {
                rows.Add(new MonthlyReportTopTransactionDto(
                    due.PayId,
                    TransactionType.Deferred,
                    due.Amount,
                    due.PlanDescription,
                    due.DueDate,
                    due.CategoryName,
                    cycle.SourceName));
            }
        }

        return rows
            .OrderByDescending(r => r.Amount)
            .Take(5)
            .ToList();
    }

    private static async Task<MonthlyReportDirectExpenseSectionDto> BuildDirectExpensesSectionAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        CancellationToken cancellationToken)
    {
        var items = await (
                from t in ExcludeBillingCyclePaymentTxns(MonthTransactions(db, year, month))
                join s in db.FinSources.AsNoTracking() on t.SourceId equals s.Id
                join c in db.FinCategories.AsNoTracking() on t.CategoryId equals c.Id into cj
                from c in cj.DefaultIfEmpty()
                where !t.IsDeleted
                      && t.Type != TransactionType.Reversal
                      && t.Type == TransactionType.Direct
                      && t.Currency == currency
                orderby t.TxnDate descending, t.CreatedAt descending
                select new MonthlyReportDirectExpenseItemDto(
                    t.Id,
                    (long)Math.Round(t.Amount, 0, MidpointRounding.AwayFromZero),
                    t.Currency,
                    t.TxnDate,
                    t.Description,
                    c != null ? c.Name : null,
                    s.Name,
                    t.SourceId,
                    t.CategoryId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var total = items.Sum(i => i.Amount);
        return new MonthlyReportDirectExpenseSectionDto(total, items.Count, items);
    }

    private static async Task<MonthlyReportBillingCyclesSectionDto> BuildBillingCyclesSectionAsync(
        IApplicationDbContext db,
        int year,
        int month,
        string currency,
        CancellationToken cancellationToken)
    {
        var cycles = await db.FinBillingCycles
            .AsNoTracking()
            .Include(c => c.Source)
            .Where(c => c.StatementDate.Year == year
                        && c.StatementDate.Month == month
                        && c.Source.Currency == currency)
            .OrderByDescending(c => c.StatementDate)
            .ThenByDescending(c => c.PeriodStart)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (cycles.Count == 0)
            return new MonthlyReportBillingCyclesSectionDto(0, 0, Array.Empty<MonthlyReportBillingCycleItemDto>());

        var cycleIds = cycles.Select(c => c.Id).ToList();
        var items = await db.FinBillingCycleItems
            .AsNoTracking()
            .Where(i => cycleIds.Contains(i.BillingCycleId) && i.RemovedAt == null)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var txnIds = items.Select(i => i.TransactionId).Distinct().ToList();
        var txnRows = txnIds.Count == 0
            ? []
            : await db.FinTransactions
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => txnIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

        var txnById = txnRows.ToDictionary(t => t.Id);
        var txnsByCycle = items
            .GroupBy(i => i.BillingCycleId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(i => txnById.GetValueOrDefault(i.TransactionId))
                    .Where(t => t is not null)
                    .Cast<FinTransaction>()
                    .OrderByDescending(t => t.TxnDate)
                    .ThenByDescending(t => t.CreatedAt)
                    .Select(t => new MonthlyReportBillingCycleTxnItemDto(
                        t.Id,
                        (long)Math.Round(t.Amount, 0, MidpointRounding.AwayFromZero),
                        t.TxnDate,
                        t.Description,
                        t.Category?.Name,
                        t.CategoryId))
                    .ToList());

        var installmentDuesByCycle = await BillingCycleInstallmentRules
            .LoadDueDtosByCycleAsync(db, cycles, cancellationToken)
            .ConfigureAwait(false);

        var cycleDtos = new List<MonthlyReportBillingCycleItemDto>(cycles.Count);
        foreach (var c in cycles)
        {
            var lines = txnsByCycle.GetValueOrDefault(c.Id) ?? [];
            var installmentRows = installmentDuesByCycle.GetValueOrDefault(c.Id) ?? [];
            var installmentDues = installmentRows
                .Select(d => new MonthlyReportBillingCycleInstallmentDueDto(
                    d.PayId,
                    d.PlanId,
                    d.PlanDescription,
                    d.CategoryName,
                    d.InstallmentNumber,
                    d.TotalInstallments,
                    d.DueDate,
                    d.Amount,
                    d.PaidAmount,
                    d.Status,
                    d.CategoryId))
                .ToList();

            cycleDtos.Add(new MonthlyReportBillingCycleItemDto(
                c.Id,
                c.SourceId,
                c.Source.Name,
                c.Name,
                c.PeriodStart,
                c.PeriodEnd,
                c.StatementDate,
                c.PaymentDueDate,
                CurrencyUnits.ToWhole(c.TotalAmount),
                CurrencyUnits.ToWhole(c.PaidAmount),
                c.Status,
                lines,
                installmentDues));
        }

        var total = cycleDtos.Sum(c => c.TotalAmount);
        return new MonthlyReportBillingCyclesSectionDto(total, cycleDtos.Count, cycleDtos);
    }

    private static (int Year, int Month) PrevMonth(int year, int month) =>
        month == 1 ? (year - 1, 12) : (year, month - 1);
}
