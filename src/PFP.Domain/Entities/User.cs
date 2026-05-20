using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>Root account record. Maps to <c>USERS</c>.</summary>
public sealed class User : SoftDeletableEntity
{
    /// <summary>Login email address. Lower-cased and unique across the platform.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Bcrypt hash of the password (cost factor 12).</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Display name shown on the profile and inside notifications.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Admin can manage members; members use finance features only.</summary>
    public UserRole Role { get; set; } = UserRole.Member;

    /// <summary>UTC timestamp of the last successful login.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary><c>false</c> blocks sign-in.</summary>
    public bool IsActive { get; set; } = true;

    public UserProfile? Profile { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

    public ICollection<UserLoginAttempt> LoginAttempts { get; set; } = new List<UserLoginAttempt>();

    public ICollection<UserAvatarUpload> AvatarUploads { get; set; } = new List<UserAvatarUpload>();

    public ICollection<UserNotificationPref> NotificationPreferences { get; set; } = new List<UserNotificationPref>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<AutomationRule> AutomationRulesCreated { get; set; } = new List<AutomationRule>();

    public ICollection<UserPasswordReset> PasswordResets { get; set; } = new List<UserPasswordReset>();

    public ICollection<UserEmailVerification> EmailVerifications { get; set; } = new List<UserEmailVerification>();

    public ICollection<UserDeletionRequest> DeletionRequests { get; set; } = new List<UserDeletionRequest>();

    public ICollection<UserDataExport> DataExports { get; set; } = new List<UserDataExport>();

    public ICollection<UserAuthProvider> AuthProviders { get; set; } = new List<UserAuthProvider>();
}
