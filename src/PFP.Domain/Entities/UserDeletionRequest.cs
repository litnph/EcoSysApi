using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// GDPR "delete my account" request, with a 30-day grace period before execution.
/// Maps to <c>USER_DELETION_REQUESTS</c>.
/// <para>
/// Lifecycle: created in <see cref="DeletionRequestStatus.Pending"/>, moves to
/// <see cref="DeletionRequestStatus.Confirmed"/> on email confirmation, and is finally anonymised by
/// the <c>ExecuteDeletionRequests</c> daily job once <see cref="ScheduledExecutionAt"/> is reached.
/// The user may transition back to <see cref="DeletionRequestStatus.Cancelled"/> at any moment before execution.
/// </para>
/// <para>
/// On execution the user's PII is overwritten in place and the row is preserved with
/// <see cref="ExecutedAt"/> set, so the deletion remains auditable.
/// </para>
/// </summary>
public sealed class UserDeletionRequest : BaseEntity
{
    /// <summary>FK to <see cref="User"/>.</summary>
    public Guid UserId { get; set; }

    /// <summary>Current step of the deletion workflow.</summary>
    public DeletionRequestStatus Status { get; set; } = DeletionRequestStatus.Pending;

    /// <summary>UTC timestamp of the email-link confirmation.</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>UTC timestamp at which the worker is allowed to anonymise (confirmation + 30 days).</summary>
    public DateTime? ScheduledExecutionAt { get; set; }

    /// <summary>SHA-256 hex of the email confirmation token (same pattern as password reset).</summary>
    public string? ConfirmationTokenHash { get; set; }

    /// <summary>After this instant the confirmation link is rejected.</summary>
    public DateTime? ConfirmationTokenExpiresAt { get; set; }

    /// <summary>UTC timestamp at which the user revoked the request.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>UTC timestamp at which the worker performed the anonymisation.</summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>Optional free-form reason supplied by the user when filing the request.</summary>
    public string? Reason { get; set; }

    // ---- Navigation ----

    public User User { get; set; } = null!;
}
