namespace PFP.Application.Common.Interfaces;

/// <summary>Queues transactional auth / GDPR emails (no-op when outbound mail is not configured).</summary>
public interface IAuthEmailDispatcher
{
    void DispatchAccountDeletionConfirmation(string email, string fullName, Guid requestId, string plainToken);

    void DispatchAccountDeletionGracePeriodNotice(string email, string fullName, DateTime scheduledExecutionAtUtc);
}
