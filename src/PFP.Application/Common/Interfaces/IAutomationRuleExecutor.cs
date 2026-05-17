using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Common.Interfaces;

/// <summary>Scoped impersonation context used while automation actions invoke MediatR handlers.</summary>
public interface IAutomationExecutionImpersonation
{
    /// <summary><c>true</c> between <see cref="Begin"/> and <see cref="Clear"/>.</summary>
    bool IsActive { get; }

    Guid? UserId { get; }

    Guid? SessionId { get; }

    Guid? OrgId { get; }

    void Begin(Guid userId, Guid sessionId, Guid orgId);

    void Clear();
}

/// <summary>Trigger-derived facts passed into condition evaluation for the current automation step.</summary>
public interface IAutomationTriggerFacts
{
    IReadOnlyDictionary<string, string>? Facts { get; set; }
}

/// <summary>Evaluates actions for a single <see cref="AutomationRule"/>.</summary>
public interface IAutomationRuleExecutor
{
    Task<AutomationExecutionResult> ExecuteRuleAsync(AutomationRule rule, CancellationToken cancellationToken);
}

/// <summary>Outcome from <see cref="IAutomationRuleExecutor.ExecuteRuleAsync"/>.</summary>
public sealed record AutomationExecutionResult(
    RunStatus Status,
    string ActionsExecutedJson,
    string? ErrorMessage,
    int DurationMs);
