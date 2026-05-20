using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>
/// Daily Hangfire job (02:00 UTC) — removes expired and long-revoked <see cref="UserSession"/> rows
/// to keep the refresh-token table bounded (spec §7.1).
/// <para>
/// Sessions are deleted (hard) when either:
/// </para>
/// <list type="bullet">
/// <item><see cref="UserSession.ExpiresAt"/> &lt; <c>UtcNow</c> — token can never be refreshed again.</item>
/// <item><see cref="UserSession.RevokedAt"/> is older than 14 days — revoked tokens are kept briefly
/// for forensic correlation, then purged.</item>
/// </list>
/// <para>
/// Emits a <see cref="SystemEventLog"/> row (<c>session.cleanup</c>) with the number of rows removed.
/// </para>
/// </summary>
public sealed class CleanupExpiredSessionsJob
{
    /// <summary>How long revoked rows are retained before purge.</summary>
    public static readonly TimeSpan RevokedGracePeriod = TimeSpan.FromDays(14);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupExpiredSessionsJob> _logger;

    /// <summary>Creates the job.</summary>
    public CleanupExpiredSessionsJob(IServiceScopeFactory scopeFactory, ILogger<CleanupExpiredSessionsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Performs one cleanup pass.</summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var now = DateTime.UtcNow;
        var revokedCutoff = now - RevokedGracePeriod;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var strategy = db.Database.CreateExecutionStrategy();
        var removed = await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var expired = await db.UserSessions
                .Where(s => s.ExpiresAt < now
                            || (s.RevokedAt != null && s.RevokedAt < revokedCutoff))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (var session in expired)
                db.UserSessions.Remove(session);

            // Spec §7.1: write a SystemEventLog row on every run — even when nothing was removed —
            // so an operator can confirm the daily 02:00 schedule actually fired.
            db.SystemEventLogs.Add(new SystemEventLog
            {
                EventType = "session.cleanup",
                JobName = nameof(CleanupExpiredSessionsJob),
                Payload = $"{{\"removed\":{expired.Count}}}",
                Status = "success",
                DurationMs = sw.ElapsedMilliseconds,
            });

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return expired.Count;
        }).ConfigureAwait(false);

        sw.Stop();

        _logger.LogInformation(
            "CleanupExpiredSessionsJob removed {RemovedCount} session row(s) in {ElapsedMs}ms.",
            removed,
            sw.ElapsedMilliseconds);
    }
}
