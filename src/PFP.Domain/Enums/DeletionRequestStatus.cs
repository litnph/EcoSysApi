namespace PFP.Domain.Enums;

/// <summary>
/// State machine for <c>USER_DELETION_REQUESTS.status</c> — implements the
/// 30-day grace-period account-deletion flow mandated by GDPR.
/// <para>
/// Allowed transitions:
/// <list type="bullet">
/// <item><see cref="Pending"/> → <see cref="Cancelled"/> (user revoked the request)</item>
/// <item><see cref="Pending"/> → <see cref="Confirmed"/> (user re-confirmed via email)</item>
/// <item><see cref="Confirmed"/> → <see cref="Cancelled"/> (still inside grace window)</item>
/// <item><see cref="Confirmed"/> → <see cref="Executed"/> (anonymisation by <c>ExecuteDeletionRequests</c> job after 30 days)</item>
/// </list>
/// </para>
/// </summary>
public enum DeletionRequestStatus
{
    /// <summary><c>pending</c> — request created, awaiting email confirmation.</summary>
    Pending = 1,

    /// <summary><c>confirmed</c> — confirmed by the user and scheduled for execution after the grace period.</summary>
    Confirmed = 2,

    /// <summary><c>cancelled</c> — cancelled by the user before execution.</summary>
    Cancelled = 3,

    /// <summary><c>executed</c> — user data has been anonymised; the row is kept for audit.</summary>
    Executed = 4,
}
