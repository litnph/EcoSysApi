using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>
/// Weekly Hangfire job — enforces audit-log retention policies (spec §7.1).
/// <para>
/// Reads every <see cref="AuditLogRetention"/> row, then for each policy:
/// </para>
/// <list type="number">
/// <item>Selects <see cref="AuditLog"/> rows whose <see cref="AuditLog.EntityType"/> matches
/// the policy (or all rows when <see cref="AuditLogRetention.EntityType"/> is <c>null</c> — the
/// global default) and whose <c>CreatedAt</c> is older than <c>UtcNow - RetainDays</c>.</item>
/// <item>If <see cref="AuditLogRetention.ArchiveBeforeDelete"/> is <c>true</c> and an
/// <see cref="AuditLogRetention.ArchiveStorageKeyPrefix"/> is set, uploads the batch as a
/// JSON Lines blob to Cloudflare R2 via <see cref="IStorageService"/> before deletion.</item>
/// <item>Hard-deletes the rows from <c>AUDIT_LOGS</c> in the same transaction as the
/// <see cref="SystemEventLog"/> book-keeping row (<c>audit.retention</c>).</item>
/// </list>
/// </summary>
public sealed class AuditLogRetentionJob
{
    /// <summary>Maximum audit rows pulled per policy per run to keep the transaction bounded.</summary>
    public const int BatchSize = 5_000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogRetentionJob> _logger;

    /// <summary>Creates the job.</summary>
    public AuditLogRetentionJob(IServiceScopeFactory scopeFactory, ILogger<AuditLogRetentionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Executes one retention pass for every active policy.</summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        List<AuditLogRetention> policies;
        await using (var policyScope = _scopeFactory.CreateAsyncScope())
        {
            var db = policyScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            policies = await db.AuditLogRetentions.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.EntityType == null)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        if (policies.Count == 0)
        {
            _logger.LogInformation("AuditLogRetentionJob has no active retention policies to apply.");
            return;
        }

        var grandTotal = 0;
        foreach (var policy in policies)
        {
            try
            {
                grandTotal += await ApplyPolicyAsync(policy, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AuditLogRetentionJob policy {PolicyId} ({EntityType}) failed.",
                    policy.Id,
                    policy.EntityType ?? "*");
            }
        }

        sw.Stop();
        _logger.LogInformation(
            "AuditLogRetentionJob removed {Total} audit row(s) across {PolicyCount} policy/policies in {ElapsedMs}ms.",
            grandTotal,
            policies.Count,
            sw.ElapsedMilliseconds);
    }

    private async Task<int> ApplyPolicyAsync(AuditLogRetention policy, CancellationToken cancellationToken)
    {
        if (policy.RetainDays <= 0)
            return 0;

        var cutoff = DateTime.UtcNow.AddDays(-policy.RetainDays);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var storage = scope.ServiceProvider.GetService<IStorageService>();

        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            var query = db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(policy.EntityType))
                query = query.Where(a => a.EntityType == policy.EntityType);

            var expiring = await query
                .Where(a => a.CreatedAt < cutoff)
                .OrderBy(a => a.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (expiring.Count == 0)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return 0;
            }

            if (policy.ArchiveBeforeDelete
                && !string.IsNullOrWhiteSpace(policy.ArchiveStorageKeyPrefix)
                && storage is not null)
            {
                var key = BuildArchiveKey(policy.ArchiveStorageKeyPrefix!, policy.EntityType);
                using var ms = new MemoryStream();
                await using (var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
                {
                    foreach (var row in expiring)
                        await writer.WriteLineAsync(JsonSerializer.Serialize(row)).ConfigureAwait(false);
                }

                ms.Position = 0;
                await storage.UploadAsync(ms, key, "application/x-ndjson", cancellationToken).ConfigureAwait(false);
            }

            db.AuditLogs.RemoveRange(expiring);

            db.SystemEventLogs.Add(new SystemEventLog
            {
                EventType = "audit.retention",
                EntityType = policy.EntityType,
                JobName = nameof(AuditLogRetentionJob),
                Payload = $"{{\"removed\":{expiring.Count},\"retain_days\":{policy.RetainDays}}}",
                Status = "success",
            });

            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return expiring.Count;
        }).ConfigureAwait(false);
    }

    private static string BuildArchiveKey(string prefix, string? entityType)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMddTHHmmss");
        var clean = prefix.TrimEnd('/');
        var bucket = string.IsNullOrWhiteSpace(entityType) ? "all" : entityType;
        return $"{clean}/{bucket}/{date}-{Guid.NewGuid():N}.jsonl";
    }
}
