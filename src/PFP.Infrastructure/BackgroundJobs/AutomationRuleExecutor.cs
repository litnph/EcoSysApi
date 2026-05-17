using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.PayBillingCycle;
using PFP.Application.Features.Savings.DepositToSaving;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Evaluates JSON conditions and dispatches JSON-defined actions through MediatR.</summary>
/// <remarks>
/// Handlers manage their own EF transactions; there is no cross-action atomic rollback — if an action fails,
/// earlier actions may already be persisted.
/// </remarks>
public sealed class AutomationRuleExecutor : IAutomationRuleExecutor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private static readonly HashSet<string> AllowedActionTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "pay_statement",
            "transfer_savings",
            "send_notification",
            "create_transaction",
        };

    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;
    private readonly IAutomationTriggerFacts _facts;
    private readonly ILogger<AutomationRuleExecutor> _logger;

    public AutomationRuleExecutor(
        IMediator mediator,
        IApplicationDbContext db,
        IAutomationTriggerFacts facts,
        ILogger<AutomationRuleExecutor> logger)
    {
        _mediator = mediator;
        _db = db;
        _facts = facts;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AutomationExecutionResult> ExecuteRuleAsync(
        AutomationRule rule,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var executed = new List<object>();

        try
        {
            var facts = _facts.Facts ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!EvaluateConditions(rule.Conditions, facts))
            {
                sw.Stop();
                var skippedJson = JsonSerializer.Serialize(new[] { new { stage = "conditions", skipped = true } }, JsonOptions);
                return new AutomationExecutionResult(RunStatus.Skipped, skippedJson, null, (int)sw.ElapsedMilliseconds);
            }

            using var actionsDoc = JsonDocument.Parse(rule.Actions);
            if (actionsDoc.RootElement.ValueKind != JsonValueKind.Array || actionsDoc.RootElement.GetArrayLength() == 0)
                throw new InvalidOperationException("Actions must be a non-empty JSON array.");

            foreach (var actionEl in actionsDoc.RootElement.EnumerateArray())
            {
                if (!actionEl.TryGetProperty("type", out var typeProp))
                    throw new InvalidOperationException("Each action requires a \"type\" property.");

                var actionType = typeProp.GetString()?.Trim();
                if (string.IsNullOrEmpty(actionType) || !AllowedActionTypes.Contains(actionType))
                    throw new InvalidOperationException($"Unsupported action type \"{actionType}\".");

                cancellationToken.ThrowIfCancellationRequested();

                switch (actionType.ToLowerInvariant())
                {
                    case "pay_statement":
                        await ExecutePayStatementAsync(actionEl, cancellationToken).ConfigureAwait(false);
                        executed.Add(new { type = actionType, ok = true });
                        break;
                    case "transfer_savings":
                        await ExecuteTransferSavingsAsync(actionEl, cancellationToken).ConfigureAwait(false);
                        executed.Add(new { type = actionType, ok = true });
                        break;
                    case "send_notification":
                        await ExecuteSendNotificationAsync(actionEl, cancellationToken).ConfigureAwait(false);
                        executed.Add(new { type = actionType, ok = true });
                        break;
                    case "create_transaction":
                        await ExecuteCreateTransactionAsync(rule, actionEl, cancellationToken).ConfigureAwait(false);
                        executed.Add(new { type = actionType, ok = true });
                        break;
                }
            }

            sw.Stop();
            var okJson = JsonSerializer.Serialize(executed, JsonOptions);
            return new AutomationExecutionResult(RunStatus.Success, okJson, null, (int)sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            executed.Add(new { error = ex.Message });
            var failJson = JsonSerializer.Serialize(executed, JsonOptions);
            _logger.LogWarning(ex, "Automation rule {RuleId} failed", rule.Id);
            return new AutomationExecutionResult(RunStatus.Failed, failJson, ex.Message, (int)sw.ElapsedMilliseconds);
        }
    }

    private static bool EvaluateConditions(string conditionsJson, IReadOnlyDictionary<string, string> facts)
    {
        using var doc = JsonDocument.Parse(conditionsJson);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var cond in doc.RootElement.EnumerateArray())
        {
            var field = cond.GetProperty("field").GetString() ?? "";
            var op = cond.GetProperty("op").GetString() ?? "";
            var expected = cond.GetProperty("value").GetString() ?? "";
            facts.TryGetValue(field, out var actual);
            actual ??= "";
            if (!EvaluateOp(actual, op, expected))
                return false;
        }

        return true;
    }

    private static bool EvaluateOp(string actual, string op, string expected)
    {
        op = op.Trim().ToLowerInvariant();
        return op switch
        {
            "eq" => string.Equals(actual, expected, StringComparison.Ordinal),
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            "gte" => CompareNumeric(actual, expected) >= 0,
            "lte" => CompareNumeric(actual, expected) <= 0,
            _ => false,
        };
    }

    private static int CompareNumeric(string left, string right)
    {
        if (!decimal.TryParse(left, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var l))
            l = 0;
        if (!decimal.TryParse(right, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var r))
            r = 0;
        return l.CompareTo(r);
    }

    private async Task ExecutePayStatementAsync(JsonElement action, CancellationToken ct)
    {
        var cycleId = RequireGuid(action, "cycleId");
        var paymentSourceId = RequireGuid(action, "paymentSourceId");
        var amount = RequireDecimal(action, "amount");
        await _mediator.Send(new PayBillingCycleCommand(cycleId, paymentSourceId, amount), ct).ConfigureAwait(false);
    }

    private async Task ExecuteTransferSavingsAsync(JsonElement action, CancellationToken ct)
    {
        var savingId = RequireGuid(action, "savingId");
        var amount = RequireDecimal(action, "amount");
        var txnDate = RequireDateOnly(action, "txnDate");
        string? note = action.TryGetProperty("note", out var n) ? n.GetString() : null;
        Guid? mp = action.TryGetProperty("monthlyPeriodId", out var mpEl) && mpEl.ValueKind == JsonValueKind.String && Guid.TryParse(mpEl.GetString(), out var mpg)
            ? mpg
            : null;
        await _mediator
            .Send(new DepositToSavingCommand(savingId, amount, txnDate, note, mp), ct)
            .ConfigureAwait(false);
    }

    private async Task ExecuteSendNotificationAsync(JsonElement action, CancellationToken ct)
    {
        var userId = RequireGuid(action, "userId");
        var type = RequireString(action, "notificationType");
        var title = RequireString(action, "title");
        var body = RequireString(action, "body");
        _db.Notifications.Add(
            new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Body = body,
                IsRead = false,
            });
        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    private async Task ExecuteCreateTransactionAsync(AutomationRule rule, JsonElement action, CancellationToken ct)
    {
        var typeStr = RequireTransactionTypeString(action);
        var txnType = ParseTransactionType(typeStr);

        var amount = RequireDecimal(action, "amount");
        var sourceId = RequireGuid(action, "sourceId");
        Guid? categoryId = OptionalGuid(action, "categoryId");
        var txnDate = RequireDateOnly(action, "txnDate");
        string? note = action.TryGetProperty("note", out var n) ? n.GetString() : null;
        Guid? monthlyPeriodId = OptionalGuid(action, "monthlyPeriodId");
        Guid? toSourceId = OptionalGuid(action, "toSourceId");
        Guid? billingCycleId = OptionalGuid(action, "billingCycleId");
        string? personName = action.TryGetProperty("personName", out var pn) ? pn.GetString() : null;
        string? personContact = action.TryGetProperty("personContact", out var pc) ? pc.GetString() : null;
        Guid? debtRecordId = OptionalGuid(action, "debtRecordId");
        DateOnly? dueDate = action.TryGetProperty("dueDate", out var dd) && dd.ValueKind == JsonValueKind.String && DateOnly.TryParse(dd.GetString(), out var due)
            ? due
            : null;

        var cmd = new CreateTransactionCommand(
            rule.SmoduleId,
            txnType,
            amount,
            sourceId,
            categoryId,
            txnDate,
            note,
            monthlyPeriodId,
            toSourceId,
            billingCycleId,
            personName,
            personContact,
            debtRecordId,
            dueDate,
            null);

        await _mediator.Send(cmd, ct).ConfigureAwait(false);
    }

    private static string RequireTransactionTypeString(JsonElement el)
    {
        if (el.TryGetProperty("transactionType", out var p))
            return p.GetString()?.Trim() ?? throw new InvalidOperationException("Missing transactionType.");
        if (el.TryGetProperty("TransactionType", out var p2))
            return p2.GetString()?.Trim() ?? throw new InvalidOperationException("Missing TransactionType.");
        throw new InvalidOperationException("Missing transactionType.");
    }

    private static TransactionType ParseTransactionType(string raw)
    {
        raw = raw.Trim();
        if (Enum.TryParse<TransactionType>(raw, ignoreCase: true, out var direct))
            return direct;

        var parts = raw.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var pascal = string.Concat(parts.Select(static p =>
            p.Length == 0 ? string.Empty : char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
        return Enum.Parse<TransactionType>(pascal, ignoreCase: true);
    }

    private static Guid RequireGuid(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            throw new InvalidOperationException($"Missing \"{name}\".");
        return Guid.Parse(p.GetString() ?? throw new InvalidOperationException($"Invalid GUID for \"{name}\"."));
    }

    private static Guid? OptionalGuid(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p) || p.ValueKind == JsonValueKind.Null)
            return null;
        return Guid.TryParse(p.GetString(), out var g) ? g : null;
    }

    private static decimal RequireDecimal(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            throw new InvalidOperationException($"Missing \"{name}\".");
        return p.ValueKind switch
        {
            JsonValueKind.Number => p.GetDecimal(),
            JsonValueKind.String => decimal.Parse(p.GetString()!, System.Globalization.CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException($"Invalid number for \"{name}\"."),
        };
    }

    private static DateOnly RequireDateOnly(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            throw new InvalidOperationException($"Missing \"{name}\".");
        return DateOnly.Parse(p.GetString()!, System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string RequireString(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var p))
            throw new InvalidOperationException($"Missing \"{name}\".");
        return p.GetString()?.Trim() ?? throw new InvalidOperationException($"Missing \"{name}\".");
    }
}
