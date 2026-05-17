using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Polymorphic link between a <see cref="User"/> and an external/internal authentication provider.
/// Maps to <c>USER_AUTH_PROVIDERS</c>.
/// <para>
/// A composite unique constraint on (<see cref="Provider"/>, <see cref="ProviderUserId"/>) prevents two
/// different platform users from sharing the same Google / Apple identity. Disabling the last active
/// provider is rejected at the handler level so the account cannot become unreachable.
/// </para>
/// </summary>
public sealed class UserAuthProvider : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Which provider this row represents.</summary>
    public AuthProvider Provider { get; set; }

    /// <summary>
    /// External identifier issued by the provider (Google <c>sub</c>, Apple user id, …).
    /// <c>null</c> for the local <see cref="AuthProvider.Email"/> provider.
    /// </summary>
    public string? ProviderUserId { get; set; }

    /// <summary>Email address as reported by the provider at link time (informational).</summary>
    public string? ProviderEmail { get; set; }

    /// <summary>Soft-disable flag; an inactive provider cannot be used to authenticate.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC timestamp at which the provider was linked to the account.</summary>
    public DateTime LinkedAt { get; set; }

    /// <summary>UTC timestamp of the most recent successful login through this provider.</summary>
    public DateTime? LastUsedAt { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
