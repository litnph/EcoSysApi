namespace PFP.Domain.Entities;

/// <summary>
/// Root account record. Maps to <c>USERS</c>.
/// <para>
/// <see cref="Email"/> is the natural login identifier and carries a unique index in the database.
/// <see cref="PasswordHash"/> is nullable so that OAuth-only accounts (Google / Apple) can exist
/// without a local credential — at least one row in <see cref="AuthProviders"/> must remain active for such users.
/// </para>
/// </summary>
public sealed class User : SoftDeletableEntity
{
    /// <summary>Login email address. Lower-cased and unique across the whole platform.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Bcrypt hash of the password (cost factor 12, per security spec §6.1).
    /// <c>null</c> for accounts created exclusively through an OAuth provider.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>Display name shown on the profile and inside notifications.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary><c>true</c> once the user has clicked the email-verification link.</summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>UTC timestamp of the last successful login. Maintained by the auth handler.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary><c>false</c> blocks sign-in (GDPR execution sets this together with soft-delete).</summary>
    public bool IsActive { get; set; } = true;

    // ---- Navigation ----

    /// <summary>1-1 profile / preferences row.</summary>
    public UserProfile? Profile { get; set; }

    /// <summary>Linked auth providers (email + OAuth).</summary>
    public ICollection<UserAuthProvider> AuthProviders { get; set; } = new List<UserAuthProvider>();

    /// <summary>Active and historical refresh-token sessions.</summary>
    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    /// <summary>Append-only login attempt audit (used by brute-force detection).</summary>
    public ICollection<UserLoginAttempt> LoginAttempts { get; set; } = new List<UserLoginAttempt>();

    /// <summary>Append-only password-reset token history.</summary>
    public ICollection<UserPasswordReset> PasswordResets { get; set; } = new List<UserPasswordReset>();

    /// <summary>Append-only email-verification token history (initial + change-email).</summary>
    public ICollection<UserEmailVerification> EmailVerifications { get; set; } = new List<UserEmailVerification>();

    /// <summary>Avatar upload history; at most one row has <c>IsActive = true</c>.</summary>
    public ICollection<UserAvatarUpload> AvatarUploads { get; set; } = new List<UserAvatarUpload>();

    /// <summary>Granular notification preferences (module × channel × event_type).</summary>
    public ICollection<UserNotificationPref> NotificationPreferences { get; set; } = new List<UserNotificationPref>();

    /// <summary>In-app notifications (billing reminders, system messages, …).</summary>
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>GDPR data-export requests submitted by this user.</summary>
    public ICollection<UserDataExport> DataExports { get; set; } = new List<UserDataExport>();

    /// <summary>GDPR deletion requests (with 30-day grace period) submitted by this user.</summary>
    public ICollection<UserDeletionRequest> DeletionRequests { get; set; } = new List<UserDeletionRequest>();

    // ---- Navigation: Layer 1 (Platform Core) ----

    /// <summary>Organisations this user owns (every user owns at least their personal organisation).</summary>
    public ICollection<Organization> OwnedOrganizations { get; set; } = new List<Organization>();

    /// <summary>Memberships across all organisations the user belongs to.</summary>
    public ICollection<OrgMember> OrgMemberships { get; set; } = new List<OrgMember>();

    /// <summary>Memberships across all spaces the user belongs to (direct and inherited).</summary>
    public ICollection<SpaceMember> SpaceMemberships { get; set; } = new List<SpaceMember>();

    /// <summary>Finance automation rules where this user is the execution principal.</summary>
    public ICollection<AutomationRule> AutomationRulesCreated { get; set; } = new List<AutomationRule>();
}
