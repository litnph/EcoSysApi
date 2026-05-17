using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>Per-scope automation impersonation + trigger facts (Hangfire job activation).</summary>
public sealed class AutomationJobEnvironment : IAutomationExecutionImpersonation, IAutomationTriggerFacts
{
    /// <inheritdoc/>
    public bool IsActive { get; private set; }

    /// <inheritdoc/>
    public Guid? UserId { get; private set; }

    /// <inheritdoc/>
    public Guid? SessionId { get; private set; }

    /// <inheritdoc/>
    public Guid? OrgId { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string>? Facts { get; set; }

    /// <inheritdoc/>
    public void Begin(Guid userId, Guid sessionId, Guid orgId)
    {
        UserId = userId;
        SessionId = sessionId;
        OrgId = orgId;
        IsActive = true;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Facts = null;
        UserId = SessionId = OrgId = null;
        IsActive = false;
    }
}
