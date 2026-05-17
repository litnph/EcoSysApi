using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>
/// Development-friendly dispatcher that never throws and never logs raw secrets — replace with a
/// real queue / SMTP integration in production.
/// </summary>
public sealed class LoggingAuthEmailDispatcher : IAuthEmailDispatcher
{
    private readonly ILogger<LoggingAuthEmailDispatcher> _logger;

    /// <summary>Creates the dispatcher.</summary>
    public LoggingAuthEmailDispatcher(ILogger<LoggingAuthEmailDispatcher> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void DispatchEmailVerification(string toEmail, string fullName, string plainVerificationToken)
    {
        _logger.LogInformation(
            "Email verification queued for {Email} ({Name}). Token length: {Length}.",
            toEmail,
            fullName,
            plainVerificationToken.Length);
    }

    /// <inheritdoc/>
    public void DispatchPasswordReset(string toEmail, string fullName, string plainResetToken)
    {
        _logger.LogInformation(
            "Password reset queued for {Email} ({Name}). Token length: {Length}.",
            toEmail,
            fullName,
            plainResetToken.Length);
    }

    /// <inheritdoc/>
    public void DispatchAccountDeletionConfirmation(string toEmail, string fullName, Guid requestId, string plainToken)
    {
        _logger.LogInformation(
            "Account deletion confirmation queued for {Email} ({Name}). RequestId={RequestId}, token length={Length}.",
            toEmail,
            fullName,
            requestId,
            plainToken.Length);
    }

    /// <inheritdoc/>
    public void DispatchAccountDeletionGracePeriodNotice(string toEmail, string fullName, DateTime scheduledExecutionAtUtc)
    {
        _logger.LogInformation(
            "Deletion grace-period notice for {Email} ({Name}). ScheduledExecutionAt={Scheduled}.",
            toEmail,
            fullName,
            scheduledExecutionAtUtc);
    }
}
