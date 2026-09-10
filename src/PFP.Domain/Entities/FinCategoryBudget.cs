using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>A user's recurring reporting-cycle budget for one expense category.</summary>
public sealed class FinCategoryBudget : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal Amount { get; set; }
    public BudgetTargetMode TargetMode { get; set; } = BudgetTargetMode.Maximum;
    public bool IsEnabled { get; set; } = true;
    public string WarningThresholdsJson { get; set; } = "[]";
    public int ConfigurationVersion { get; set; } = 1;

    public User User { get; set; } = null!;
    public FinCategory Category { get; set; } = null!;
}
