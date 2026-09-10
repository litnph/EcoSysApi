using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Identifies and persists the single monthly report that owns a transaction.</summary>
internal static class MonthlyReportTransactionOwnership
{
    internal readonly record struct ReportIdentity(Guid PeriodId, DateTime ReportCreatedAt);

    public static async Task<HashSet<Guid>> GetExcludedTransactionIdsAsync(
        IApplicationDbContext db,
        int year,
        int month,
        ReportIdentity? identity,
        CancellationToken cancellationToken)
    {
        var current = identity ?? await db.FinMonthlyPeriods
            .AsNoTracking()
            .Where(period => period.Year == year
                             && period.Month == month
                             && period.ReportCreatedAt != null)
            .Select(period => new ReportIdentity(period.Id, period.ReportCreatedAt!.Value))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (current.PeriodId == Guid.Empty)
            return [];

        var linkedToOtherPeriods = await db.FinTransactions
            .AsNoTracking()
            .Where(transaction => transaction.MonthlyPeriodId != null
                                  && transaction.MonthlyPeriodId != current.PeriodId)
            .Select(transaction => transaction.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var excluded = linkedToOtherPeriods.ToHashSet();

        var otherReports = await db.FinMonthlyPeriods
            .AsNoTracking()
            .Where(period => period.Id != current.PeriodId
                             && period.ReportCreatedAt != null
                             && period.ReportSnapshot != null)
            .Select(period => new
            {
                period.Id,
                ReportCreatedAt = period.ReportCreatedAt!.Value,
                Snapshot = period.ReportSnapshot!,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var report in otherReports)
        {
            var wasCreatedEarlier = report.ReportCreatedAt < current.ReportCreatedAt
                || (report.ReportCreatedAt == current.ReportCreatedAt
                    && string.CompareOrdinal(report.Id.ToString("N"), current.PeriodId.ToString("N")) < 0);
            if (!wasCreatedEarlier)
                continue;

            excluded.UnionWith(ExtractTransactionIds(
                MonthlyReportSnapshotStore.Deserialize(report.Snapshot)));
        }

        return excluded;
    }

    public static HashSet<Guid> ExtractTransactionIds(MonthlyReportDto report)
    {
        var result = new HashSet<Guid>();
        var groups = report.CurrencyGroups is { Count: > 0 }
            ? report.CurrencyGroups
            : [new MonthlyReportCurrencyGroupDto(
                report.Metadata?.Currency ?? "VND",
                report.Summary,
                report.CategoryBreakdown,
                report.SourceBreakdown,
                report.TopTransactions,
                report.DailyBreakdown,
                report.ComparisonWithPreviousMonth,
                report.DirectExpenses,
                report.BillingCycles,
                report.BudgetUtilizations)];

        foreach (var group in groups)
            result.UnionWith(group.DirectExpenses.Items.Select(item => item.Id));

        return result;
    }

    public static async Task SynchronizeAsync(
        IApplicationDbContext db,
        Guid periodId,
        MonthlyReportDto report,
        CancellationToken cancellationToken)
    {
        var includedIds = ExtractTransactionIds(report);
        var rows = await db.FinTransactions
            .Where(transaction => transaction.Type == TransactionType.Direct
                                  && (transaction.MonthlyPeriodId == periodId
                                      || includedIds.Contains(transaction.Id)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var transaction in rows)
        {
            if (includedIds.Contains(transaction.Id))
            {
                if (transaction.MonthlyPeriodId is null || transaction.MonthlyPeriodId == periodId)
                    transaction.MonthlyPeriodId = periodId;
            }
            else if (transaction.MonthlyPeriodId == periodId)
            {
                transaction.MonthlyPeriodId = null;
            }
        }
    }
}
