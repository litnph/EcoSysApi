namespace PFP.Application.Features.Notifications.Common;

public interface IBudgetAlertEvaluator
{
    Task EvaluateCurrentCycleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task EvaluateTargetMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken = default);
}
