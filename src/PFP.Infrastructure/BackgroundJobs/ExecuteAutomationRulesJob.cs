using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Infrastructure.Identity;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Hourly evaluation of finance automation rules.</summary>
public sealed class ExecuteAutomationRulesJob
{
    private readonly IApplicationDbContext _db;
    private readonly IAutomationRuleExecutor _executor;
    private readonly AutomationJobEnvironment _environment;
    private readonly ILogger<ExecuteAutomationRulesJob> _logger;

    public ExecuteAutomationRulesJob(
        IApplicationDbContext db,
        IAutomationRuleExecutor executor,
        AutomationJobEnvironment environment,
        ILogger<ExecuteAutomationRulesJob> logger)
    {
        _db = db;
        _executor = executor;
        _environment = environment;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var hourAgo = utcNow.AddHours(-1);

        var rules = await _db.AutomationRules
            .Include(r => r.Smodule)
            .ThenInclude(m => m.Space)
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var rule in rules)
        {
            try
            {
                if (ShouldSkipDailySuccessful(rule, utcNow))
                    continue;

                var match = await TryMatchTriggerAsync(rule, today, hourAgo, utcNow, cancellationToken).ConfigureAwait(false);
                if (!match.Matched)
                {
                    AppendLog(rule.Id, utcNow, RunStatus.Skipped, "[]", "Trigger condition not matched.", 0);
                    rule.LastRunAt = utcNow;
                    rule.LastRunStatus = RunStatus.Skipped;
                    continue;
                }

                var space = rule.Smodule.Space;
                _environment.Begin(rule.CreatedByUserId, AutomationBackgroundConstants.SyntheticSessionId, space.OrgId);
                try
                {
                    _environment.Facts = match.Facts;

                    var result = await _executor.ExecuteRuleAsync(rule, cancellationToken).ConfigureAwait(false);
                    rule.LastRunAt = utcNow;
                    rule.LastRunStatus = result.Status;
                    AppendLog(rule.Id, utcNow, result.Status, result.ActionsExecutedJson, result.ErrorMessage, result.DurationMs);
                }
                finally
                {
                    _environment.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Automation rule job failed for rule {RuleId}", rule.Id);
                rule.LastRunAt = utcNow;
                rule.LastRunStatus = RunStatus.Failed;
                AppendLog(rule.Id, utcNow, RunStatus.Failed, "[]", ex.Message, 0);
            }
        }

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static bool ShouldSkipDailySuccessful(AutomationRule rule, DateTime utcNow)
    {
        if (rule.LastRunAt is null || rule.LastRunStatus != RunStatus.Success)
            return false;

        return rule.TriggerType switch
        {
            TriggerType.DebtDue or TriggerType.FixedDate => rule.LastRunAt.Value.Date == utcNow.Date,
            _ => false,
        };
    }

    private void AppendLog(
        Guid ruleId,
        DateTime triggeredAt,
        RunStatus status,
        string actionsJson,
        string? error,
        int durationMs)
    {
        var logAt = DateTime.UtcNow;
        _db.AutomationLogs.Add(
            new AutomationLog
            {
                RuleId = ruleId,
                TriggeredAt = triggeredAt,
                Status = status,
                ActionsExecuted = actionsJson,
                ErrorMessage = error,
                DurationMs = durationMs,
            });
    }

    private async Task<TriggerMatch> TryMatchTriggerAsync(
        AutomationRule rule,
        DateOnly today,
        DateTime hourAgo,
        DateTime utcNow,
        CancellationToken ct)
    {
        switch (rule.TriggerType)
        {
            case TriggerType.IncomeReceived:
            {
                var txn = await _db.FinTransactions
                    .AsNoTracking()
                    .Where(t =>
                        t.SmoduleId == rule.SmoduleId
                        && t.Type == TransactionType.Income
                        && !t.IsDeleted
                        && t.CreatedAt >= hourAgo)
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (txn is null)
                    return TriggerMatch.No();

                var facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["amount"] = txn.Amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["currency"] = txn.Currency,
                    ["transactionId"] = txn.Id.ToString(),
                    ["sourceId"] = txn.SourceId.ToString(),
                };
                if (txn.CategoryId is { } catId)
                    facts["categoryId"] = catId.ToString();
                return TriggerMatch.Yes(facts);
            }

            case TriggerType.StatementDate:
            {
                var cycle = await _db.FinBillingCycles
                    .AsNoTracking()
                    .Where(c =>
                        c.SmoduleId == rule.SmoduleId
                        && c.Status == BillingCycleStatus.Closed
                        && c.ClosedAt != null
                        && c.ClosedAt >= hourAgo)
                    .OrderByDescending(c => c.ClosedAt)
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (cycle is null)
                    return TriggerMatch.No();

                var facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["cycleId"] = cycle.Id.ToString(),
                    ["totalAmount"] = cycle.TotalAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["paidAmount"] = cycle.PaidAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["sourceId"] = cycle.SourceId.ToString(),
                };
                return TriggerMatch.Yes(facts);
            }

            case TriggerType.FixedDate:
            {
                if (!DateOnly.TryParse(rule.TriggerValue.Trim(), System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out var target)
                    && !DateOnly.TryParse(rule.TriggerValue.Trim(), out target))
                    return TriggerMatch.No();

                if (target != today)
                    return TriggerMatch.No();

                return TriggerMatch.Yes(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["today"] = today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                });
            }

            case TriggerType.DebtDue:
            {
                var debt = await _db.FinDebtRecords
                    .AsNoTracking()
                    .Where(d =>
                        d.SmoduleId == rule.SmoduleId
                        && !d.IsDeleted
                        && d.Status == DebtStatus.Active
                        && d.RemainingAmount > 0
                        && d.DueDate == today)
                    .OrderByDescending(d => d.RemainingAmount)
                    .FirstOrDefaultAsync(ct)
                    .ConfigureAwait(false);

                if (debt is null)
                    return TriggerMatch.No();

                var facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["debtRecordId"] = debt.Id.ToString(),
                    ["remainingAmount"] = debt.RemainingAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["currency"] = debt.Currency,
                    ["personName"] = debt.PersonName,
                };
                return TriggerMatch.Yes(facts);
            }

            default:
                return TriggerMatch.No();
        }
    }

    private readonly record struct TriggerMatch(bool Matched, IReadOnlyDictionary<string, string>? Facts)
    {
        public static TriggerMatch No() => new(false, null);

        public static TriggerMatch Yes(IReadOnlyDictionary<string, string> facts) => new(true, facts);
    }
}
