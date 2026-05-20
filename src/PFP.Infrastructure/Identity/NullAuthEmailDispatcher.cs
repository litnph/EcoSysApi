using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Identity;

/// <summary>No-op email dispatcher used when outbound mail is not wired.</summary>
public sealed class NullAuthEmailDispatcher : IAuthEmailDispatcher
{
    private readonly ILogger<NullAuthEmailDispatcher> _logger;

    public NullAuthEmailDispatcher(ILogger<NullAuthEmailDispatcher> logger) => _logger = logger;

    public void DispatchAccountDeletionConfirmation(string email, string fullName, Guid requestId, string plainToken) =>
        _logger.LogInformation(
            "Account deletion confirmation for {Email} (request {RequestId}) — email dispatch is disabled.",
            email,
            requestId);

    public void DispatchAccountDeletionGracePeriodNotice(string email, string fullName, DateTime scheduledExecutionAtUtc) =>
        _logger.LogInformation(
            "Account deletion grace-period notice for {Email} scheduled at {ScheduledAt} — email dispatch is disabled.",
            email,
            scheduledExecutionAtUtc);
}
