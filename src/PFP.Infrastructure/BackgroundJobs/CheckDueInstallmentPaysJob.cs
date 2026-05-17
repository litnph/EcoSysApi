using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Daily job: marks installment pays due/overdue and notifies org owners.</summary>
public sealed class CheckDueInstallmentPaysJob
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IApplicationDbContext _db;
    private readonly ILogger<CheckDueInstallmentPaysJob> _logger;

    /// <summary>Creates the job.</summary>
    public CheckDueInstallmentPaysJob(IApplicationDbContext db, ILogger<CheckDueInstallmentPaysJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Updates pay statuses and sends overdue notifications.</summary>
    public async Task Execute(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await _db.FinInstallmentPays
            .Where(p => p.Status == InstallmentPayStatus.Upcoming && p.DueDate <= today)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Status, InstallmentPayStatus.Due),
                cancellationToken)
            .ConfigureAwait(false);

        var overdueCandidates = await _db.FinInstallmentPays
            .AsNoTracking()
            .Where(p => p.Status == InstallmentPayStatus.Due && p.DueDate < today)
            .Select(p => new
            {
                p.Id,
                OwnerId = p.Plan.Smodule.Space.Org.OwnerId,
                SourceName = p.Plan.Source.Name,
                p.DueDate,
                p.InstallmentNumber,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await _db.FinInstallmentPays
            .Where(p => p.Status == InstallmentPayStatus.Due && p.DueDate < today)
            .ExecuteUpdateAsync(
                s => s.SetProperty(p => p.Status, InstallmentPayStatus.Overdue),
                cancellationToken)
            .ConfigureAwait(false);

        var vi = CultureInfo.GetCultureInfo("vi-VN");
        foreach (var row in overdueCandidates)
        {
            var days = today.DayNumber - row.DueDate.DayNumber;
            var body =
                $"Kỳ trả góp số {row.InstallmentNumber} trên thẻ [{row.SourceName}] đã quá hạn {days} ngày (hạn {row.DueDate.ToString("dd/MM/yyyy", vi)}).";
            _db.Notifications.Add(
                new Notification
                {
                    UserId = row.OwnerId,
                    Type = "installment_overdue",
                    Title = "Trả góp quá hạn",
                    Body = body.Length > 2048 ? body[..2048] : body,
                    IsRead = false,
                });
        }

        var runAt = DateTime.UtcNow;
        var payload = JsonSerializer.Serialize(
            new
            {
                run_at = runAt,
                upcoming_to_due = true,
                overdue_notified = overdueCandidates.Count,
            },
            JsonOptions);

        _db.SystemEventLogs.Add(
            new SystemEventLog
            {
                EventType = "hangfire.check_due_installment_pays.completed",
                JobName = "CheckDueInstallmentPays",
                Payload = payload,
                Status = "success",
                CreatedAt = runAt,
                UpdatedAt = runAt,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation(
            "CheckDueInstallmentPays completed; overdue notifications: {Count}",
            overdueCandidates.Count);
    }
}
