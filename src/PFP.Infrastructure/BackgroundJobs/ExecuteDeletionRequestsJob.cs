using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
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
        var sw = Stopwatch.StartNew();
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

        var executed = 0;
        var failed = 0;
        foreach (var id in requestIds)
        {
            try
            {
                await ProcessOneAsync(id, cancellationToken).ConfigureAwait(false);
                executed++;
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex, "Failed executing account deletion request {RequestId}", id);
            }
        }

        sw.Stop();

        // Spec §7.1: every Hangfire job must append a SystemEventLog row on completion (success or
        // partial failure) so operators can audit the daily 03:00 GDPR-anonymisation pass.
        await using var summaryScope = _scopeFactory.CreateAsyncScope();
        var summaryDb = summaryScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        summaryDb.SystemEventLogs.Add(new SystemEventLog
        {
            EventType = "hangfire.execute_deletion_requests.completed",
            JobName = nameof(ExecuteDeletionRequestsJob),
            Payload = $"{{\"candidates\":{requestIds.Count},\"executed\":{executed},\"failed\":{failed}}}",
            Status = failed == 0 ? "success" : "partial",
            DurationMs = sw.ElapsedMilliseconds,
        });
        await summaryDb.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
