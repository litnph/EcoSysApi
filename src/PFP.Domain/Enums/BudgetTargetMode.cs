namespace PFP.Domain.Enums;

/// <summary>How an actual category amount is compared with its configured target.</summary>
public enum BudgetTargetMode
{
    /// <summary>The actual amount should not exceed the target.</summary>
    Maximum = 1,

    /// <summary>The actual amount should reach at least the target.</summary>
    Minimum = 2,
}
