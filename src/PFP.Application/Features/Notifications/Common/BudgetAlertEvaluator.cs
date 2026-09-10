using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.Budgets.Common;
using PFP.Application.Features.MonthlyPeriods.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Notifications.Common;

/// <summary>Evaluates budget state under a row lock and persists consumed keys plus at most one alert per budget.</summary>
public sealed class BudgetAlertEvaluator : IBudgetAlertEvaluator
{
    private const string ReachedKey = "limit:reached";
    private const string ExceededKey = "limit:exceeded";
    private const string TargetAchievedKey = "target:achieved";
    private const string TargetExceededKey = "target:exceeded";

    private readonly IApplicationDbContext _db;

    public BudgetAlertEvaluator(IApplicationDbContext db) => _db = db;

    public async Task EvaluateCurrentCycleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var reportDay = await MonthlyReportUserPreferences
            .GetReportDayAsync(_db, userId, cancellationToken)
            .ConfigureAwait(false);
        var target = ReportingPeriodCalculator.TargetMonthContaining(
            FinanceBusinessCalendar.Today,
            reportDay);
        await EvaluateTargetMonthAsync(userId, target.Year, target.Month, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task EvaluateTargetMonthAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default) =>
        DbTransactionRunner.ExecuteAsync(_db, async ct =>
        {
            var budgets = await _db.FinCategoryBudgets
                .FromSqlInterpolated(
                    $"SELECT * FROM fin_category_budgets WITH (UPDLOCK, ROWLOCK) WHERE user_id = {userId} AND is_enabled = 1")
                .Include(budget => budget.Category)
                .ToListAsync(ct)
                .ConfigureAwait(false);
            if (budgets.Count == 0)
                return;

            var reportDay = await MonthlyReportUserPreferences
                .GetReportDayAsync(_db, userId, ct)
                .ConfigureAwait(false);
            var period = ReportingPeriodCalculator.ForTargetMonth(year, month, reportDay);
            var report = await MonthlyPeriodSummaryCalculator
                .BuildReportAsync(_db, year, month, userId, reportDay, ct)
                .ConfigureAwait(false);
            var spentByBudget = (report.CurrencyGroups ?? Array.Empty<MonthlyReportCurrencyGroupDto>())
                .SelectMany(group => group.CategoryBreakdown
                    .Where(row => row.CategoryId.HasValue)
                    .Select(row => new
                    {
                        row.CategoryId,
                        group.Currency,
                        Amount = CurrencyUnits.FromWhole(row.Amount),
                    }))
                .ToDictionary(
                    row => (row.CategoryId!.Value, row.Currency),
                    row => row.Amount);

            var existingStates = await _db.FinBudgetAlertStates
                .Where(state => state.UserId == userId
                                && state.CycleStart == period.StartInclusive
                                && state.CycleEnd == period.EndExclusive)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            foreach (var budget in budgets)
            {
                var spent = spentByBudget.GetValueOrDefault((budget.CategoryId, budget.Currency));
                var thresholds = BudgetThresholdJson.Deserialize(budget.WarningThresholdsJson);
                var evaluation = BudgetTargetRules.Evaluate(
                    spent,
                    budget.Amount,
                    budget.TargetMode,
                    thresholds);
                var crossed = thresholds
                    .Where(threshold => evaluation.ProgressPercent >= threshold)
                    .Select(threshold => new AlertCandidate(
                        WarningKey(budget.TargetMode, threshold),
                        budget.TargetMode == BudgetTargetMode.Maximum
                            ? NotificationKind.BudgetWarning
                            : NotificationKind.TargetProgress,
                        threshold,
                        1))
                    .ToList();

                if (evaluation.ProgressPercent >= 100m)
                {
                    crossed.Add(budget.TargetMode == BudgetTargetMode.Maximum
                        ? new AlertCandidate(ReachedKey, NotificationKind.BudgetReached, null, 2)
                        : new AlertCandidate(TargetAchievedKey, NotificationKind.TargetAchieved, null, 2));
                }
                if (evaluation.ProgressPercent > 100m)
                {
                    crossed.Add(budget.TargetMode == BudgetTargetMode.Maximum
                        ? new AlertCandidate(ExceededKey, NotificationKind.BudgetExceeded, null, 3)
                        : new AlertCandidate(TargetExceededKey, NotificationKind.TargetExceeded, null, 3));
                }

                if (crossed.Count == 0)
                    continue;

                var existingKeys = existingStates
                    .Where(state => state.CategoryId == budget.CategoryId
                                    && state.Currency == budget.Currency
                                    && state.ConfigurationVersion == budget.ConfigurationVersion)
                    .Select(state => state.AlertKey)
                    .ToHashSet(StringComparer.Ordinal);
                var newlyCrossed = crossed
                    .Where(candidate => !existingKeys.Contains(candidate.Key))
                    .ToList();
                if (newlyCrossed.Count == 0)
                    continue;

                var highest = newlyCrossed
                    .OrderByDescending(candidate => candidate.Severity)
                    .ThenByDescending(candidate => candidate.ThresholdPercent ?? 100m)
                    .First();

                foreach (var candidate in newlyCrossed)
                {
                    var state = new FinBudgetAlertState
                    {
                        UserId = userId,
                        CategoryId = budget.CategoryId,
                        Currency = budget.Currency,
                        CycleStart = period.StartInclusive,
                        CycleEnd = period.EndExclusive,
                        ConfigurationVersion = budget.ConfigurationVersion,
                        AlertKey = candidate.Key,
                        WasNotified = candidate.Key == highest.Key,
                    };
                    _db.FinBudgetAlertStates.Add(state);
                    existingStates.Add(state);
                }

                _db.UserNotifications.Add(new UserNotification
                {
                    UserId = userId,
                    Kind = highest.Kind,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category.Name,
                    Currency = budget.Currency,
                    SpentAmount = spent,
                    BudgetAmount = budget.Amount,
                    UtilizationPercent = evaluation.ProgressPercent,
                    ThresholdPercent = highest.ThresholdPercent,
                    CycleStart = period.StartInclusive,
                    CycleEnd = period.EndExclusive,
                    IsRead = false,
                });
            }

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
        }, cancellationToken);

    private static string WarningKey(BudgetTargetMode mode, decimal threshold) =>
        mode == BudgetTargetMode.Maximum
            ? $"warning:{threshold.ToString("0.00", CultureInfo.InvariantCulture)}"
            : $"target:progress:{threshold.ToString("0.00", CultureInfo.InvariantCulture)}";

    private sealed record AlertCandidate(
        string Key,
        NotificationKind Kind,
        decimal? ThresholdPercent,
        int Severity);
}
