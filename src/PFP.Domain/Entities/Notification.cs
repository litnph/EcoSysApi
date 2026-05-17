namespace PFP.Domain.Entities;

/// <summary>
/// In-app notification delivered to a single user. Maps to <c>notifications</c>.
/// </summary>
public sealed class Notification : BaseEntity
{
    /// <summary>Recipient user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Machine-readable type (e.g. <c>billing_overdue</c>).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Short headline shown in the notification centre.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Longer human-readable body.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary><c>false</c> until the user dismisses or opens the notification.</summary>
    public bool IsRead { get; set; }

    public User User { get; set; } = null!;
}
