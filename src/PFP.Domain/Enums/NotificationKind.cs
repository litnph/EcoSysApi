namespace PFP.Domain.Enums;

/// <summary>Persistent in-app notification classifications.</summary>
public enum NotificationKind
{
    BudgetWarning = 1,
    BudgetReached = 2,
    BudgetExceeded = 3,
    TargetProgress = 4,
    TargetAchieved = 5,
    TargetExceeded = 6,
}
