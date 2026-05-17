namespace PFP.Domain.Enums;

/// <summary>
/// Delivery channel for a user notification preference.
/// Combined with <see cref="ModuleCode"/> and an <c>event_type</c> string to form the granular preference matrix
/// described in the <c>USER_NOTIFICATION_PREFS</c> table (module_code × channel × event_type).
/// </summary>
public enum NotificationChannel
{
    /// <summary><c>in_app</c> — bell-icon notifications inside the application.</summary>
    InApp = 1,

    /// <summary><c>email</c> — transactional email delivered via the configured provider (Resend).</summary>
    Email = 2,

    /// <summary><c>push</c> — mobile / browser push notification.</summary>
    Push = 3,

    /// <summary><c>sms</c> — SMS delivery (reserved; not wired in MVP).</summary>
    Sms = 4,
}
