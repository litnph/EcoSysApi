using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>Persistent, user-owned in-app notification with structured budget context.</summary>
public sealed class UserNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationKind Kind { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Currency { get; set; }
    public decimal? SpentAmount { get; set; }
    public decimal? BudgetAmount { get; set; }
    public decimal? UtilizationPercent { get; set; }
    public decimal? ThresholdPercent { get; set; }
    public DateOnly? CycleStart { get; set; }
    public DateOnly? CycleEnd { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
    public FinCategory? Category { get; set; }
}
