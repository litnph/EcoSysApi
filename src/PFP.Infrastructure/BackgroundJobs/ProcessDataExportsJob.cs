using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Builds JSON GDPR bundles and uploads them to R2.</summary>
public sealed class ProcessDataExportsJob
{
    private static readonly TimeSpan DownloadTtl = TimeSpan.FromDays(7);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    static ProcessDataExportsJob()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    }

    private readonly IApplicationDbContext _db;
    private readonly IUserDataExportStorage _storage;
    private readonly ILogger<ProcessDataExportsJob> _logger;

    /// <summary>Creates the job.</summary>
    public ProcessDataExportsJob(
        IApplicationDbContext db,
        IUserDataExportStorage storage,
        ILogger<ProcessDataExportsJob> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    /// <summary>Hangfire recurring sweep — processes the oldest pending export.</summary>
    public async Task ProcessNextPendingAsync(CancellationToken cancellationToken = default)
    {
        var next = await _db.UserDataExports.AsNoTracking()
            .Where(e => e.Status == DataExportStatus.Pending)
            .OrderBy(e => e.CreatedAt)
            .Select(e => (Guid?)e.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (next is null)
            return;

        await ProcessSingleExportAsync(next.Value, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Processes one export by id (enqueue target).</summary>
    public async Task ProcessSingleExportAsync(Guid exportId, CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var export = await _db.UserDataExports
            .FirstOrDefaultAsync(e => e.Id == exportId, cancellationToken)
            .ConfigureAwait(false);

        if (export is null)
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (export.Status is DataExportStatus.Ready or DataExportStatus.Expired or DataExportStatus.Failed)
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        export.Status = DataExportStatus.Processing;
        export.ProcessedAt = DateTime.UtcNow;
        export.ErrorMessage = null;

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var userId = export.UserId;
            var payload = await BuildExportPayloadAsync(userId, cancellationToken).ConfigureAwait(false);
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);

            var objectKey = $"exports/{userId:N}/{exportId:N}.json";
            await _storage.PutJsonObjectAsync(objectKey, bytes, cancellationToken).ConfigureAwait(false);
            var url = _storage.CreatePresignedGetUrl(objectKey, DownloadTtl);

            var now = DateTime.UtcNow;
            export.Status = DataExportStatus.Ready;
            export.StorageKey = objectKey;
            export.DownloadUrl = url;
            export.SizeBytes = bytes.LongLength;
            export.ReadyAt = now;
            export.ExpiresAt = now.Add(DownloadTtl);

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GDPR export {ExportId} failed for user {UserId}", exportId, export.UserId);
            export.Status = DataExportStatus.Failed;
            export.ErrorMessage = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<object> BuildExportPayloadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var smoduleIds = await (
                from sm in _db.SpaceMembers.AsNoTracking()
                join sp in _db.Spaces.AsNoTracking() on sm.SpaceId equals sp.Id
                join mod in _db.SpaceModules.AsNoTracking() on sp.Id equals mod.SpaceId
                where sm.UserId == userId
                      && !sm.IsDeleted
                      && !sp.IsDeleted
                      && !mod.IsDeleted
                      && mod.ModuleCode == ModuleCode.Finance
                      && mod.IsEnabled
                select mod.Id)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var sources = await _db.FinSources.AsNoTracking()
            .Where(s => smoduleIds.Contains(s.SmoduleId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var categories = await _db.FinCategories.AsNoTracking()
            .Where(c => smoduleIds.Contains(c.SmoduleId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var transactions = await _db.FinTransactions.AsNoTracking()
            .Where(t => smoduleIds.Contains(t.SmoduleId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var debtRecords = await _db.FinDebtRecords.AsNoTracking()
            .Include(d => d.FinDebtTransactions)
            .Where(d => smoduleIds.Contains(d.SmoduleId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var installmentPlans = await _db.FinInstallmentPlans.AsNoTracking()
            .Include(p => p.Pays)
            .Where(p => smoduleIds.Contains(p.SmoduleId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var auditLogs = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new
        {
            schema_version = 1,
            user_id = userId,
            generated_at_utc = DateTime.UtcNow,
            finance_module_ids = smoduleIds,
            sources,
            categories,
            transactions,
            debt_records = debtRecords,
            installment_plans = installmentPlans,
            audit_logs = auditLogs,
        };
    }
}
