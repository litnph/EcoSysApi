namespace PFP.Infrastructure.BackgroundJobs;

internal static class AutomationBackgroundConstants
{
    /// <summary>Synthetic session id used only for automation-triggered MediatR commands.</summary>
    internal static readonly Guid SyntheticSessionId = Guid.Parse("a0000000-0000-4000-8000-000000000001");
}
