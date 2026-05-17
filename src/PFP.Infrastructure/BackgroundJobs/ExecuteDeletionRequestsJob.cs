using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Daily job: executes confirmed GDPR deletion requests after the grace period.</summary>
public sealed class ExecuteDeletionRequestsJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExecuteDeletionRequestsJob> _logger;

    /// <summary>Creates the job.</summary>
    public ExecuteDeletionRequestsJob(
        IServiceScopeFactory scopeFactory,
        ILogger<ExecuteDeletionRequestsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Processes all due deletion requests (each in its own scope / transaction).</summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        List<Guid> requestIds;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            requestIds = await db.UserDeletionRequests.AsNoTracking()
                .Where(r =>
                    r.Status == DeletionRequestStatus.Confirmed
                    && r.ScheduledExecutionAt != null
                    && r.ScheduledExecutionAt <= DateTime.UtcNow)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var id in requestIds)
        {
            try
            {
                await ProcessOneAsync(id, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed executing account deletion request {RequestId}", id);
            }
        }
    }

    private async Task ProcessOneAsync(Guid requestId, CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var req = await db.UserDeletionRequests
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
                .ConfigureAwait(false);

            if (req is null
                || req.Status != DeletionRequestStatus.Confirmed
                || req.ScheduledExecutionAt is null
                || req.ScheduledExecutionAt > DateTime.UtcNow)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            var user = await db.Users
                .Include(u => u.Sessions)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken)
                .ConfigureAwait(false);

            if (user is null)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            var providers = await db.UserAuthProviders
                .Where(p => p.UserId == user.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            db.UserAuthProviders.RemoveRange(providers);

            user.Email = $"deleted_{user.Id:N}@deleted.com";
            user.FullName = "Deleted User";
            user.PasswordHash = null;
            user.IsActive = false;
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = null;

            foreach (var session in user.Sessions)
            {
                if (session.RevokedAt == null)
                    session.RevokedAt = DateTime.UtcNow;
            }

            req.Status = DeletionRequestStatus.Executed;
            req.ExecutedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }
}
