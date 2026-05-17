namespace PFP.Domain.Enums;

/// <summary>Outcome of an automation rule evaluation or action run.</summary>
public enum RunStatus
{
    Success = 1,

    Failed = 2,

    Skipped = 3,
}
