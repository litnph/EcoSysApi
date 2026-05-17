using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Granular per-user notification preference. Maps to <c>USER_NOTIFICATION_PREFS</c>.
/// <para>
/// Each row toggles delivery for a specific
/// (<see cref="ModuleCode"/> × <see cref="Channel"/> × <see cref="EventType"/>) combination,
/// matching the matrix described by the spec. A composite unique index on those three columns
/// (plus <see cref="UserId"/>) prevents duplicate rules; missing rows default to "enabled".
/// </para>
/// </summary>
public sealed class UserNotificationPref : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Module that produces the notification (e.g. <see cref="Enums.ModuleCode.Finance"/>).</summary>
    public ModuleCode ModuleCode { get; set; }

    /// <summary>Delivery channel.</summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Event identifier inside the module — free-form snake_case string,
    /// e.g. <c>bill_due</c>, <c>billing_cycle_overdue</c>, <c>monthly_period_closed</c>.
    /// Centralised constants live alongside the producing handler.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Whether the channel is opted-in for this event.</summary>
    public bool IsEnabled { get; set; } = true;

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
