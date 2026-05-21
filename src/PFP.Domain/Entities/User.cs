using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>Root account record. Maps to <c>USERS</c>.</summary>
public sealed class User : SoftDeletableEntity
{
    /// <summary>Login email address. Lower-cased and unique across the platform.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Bcrypt hash of the password (cost factor 12).</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Display name shown on the profile.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Admin can manage members; members use finance features only.</summary>
    public UserRole Role { get; set; } = UserRole.Member;

    /// <summary>UTC timestamp of the last successful login.</summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary><c>false</c> blocks sign-in.</summary>
    public bool IsActive { get; set; } = true;

    public UserProfile? Profile { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}
