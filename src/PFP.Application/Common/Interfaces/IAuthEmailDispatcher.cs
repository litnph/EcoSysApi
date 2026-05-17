namespace PFP.Application.Common.Interfaces;

/// <summary>
/// Dispatches transactional emails for auth flows. Implementations must not throw back into the HTTP
/// pipeline after registration / forgot-password commits (spec §4.1 step 8).
/// </summary>
public interface IAuthEmailDispatcher
{
    /// <summary>Queues or sends the post-registration email verification message.</summary>
    void DispatchEmailVerification(string toEmail, string fullName, string plainVerificationToken);

    /// <summary>Queues or sends the password-reset link email.</summary>
    void DispatchPasswordReset(string toEmail, string fullName, string plainResetToken);

    /// <summary>Sends GDPR account-deletion email with confirmation link token.</summary>
    void DispatchAccountDeletionConfirmation(string toEmail, string fullName, Guid requestId, string plainToken);

    /// <summary>Notifies user that deletion is scheduled after grace period.</summary>
    void DispatchAccountDeletionGracePeriodNotice(
        string toEmail,
        string fullName,
        DateTime scheduledExecutionAtUtc);
}
