namespace PFP.Domain.Entities;

/// <summary>Durable consumed/notified state for one budget alert identity in a concrete cycle.</summary>
public sealed class FinBudgetAlertState : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public string Currency { get; set; } = "VND";
    public DateOnly CycleStart { get; set; }
    public DateOnly CycleEnd { get; set; }
    public int ConfigurationVersion { get; set; }
    public string AlertKey { get; set; } = string.Empty;
    public bool WasNotified { get; set; }

    public User User { get; set; } = null!;
    public FinCategory Category { get; set; } = null!;
}
