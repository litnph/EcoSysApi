using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Daily job: marks closed cycles past payment due as overdue and notifies the org owner.</summary>
public sealed class CheckOverdueBillingCyclesJob
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IApplicationDbContext _db;
    private readonly ILogger<CheckOverdueBillingCyclesJob> _logger;

    /// <summary>Creates the job.</summary>
    public CheckOverdueBillingCyclesJob(IApplicationDbContext db, ILogger<CheckOverdueBillingCyclesJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>Promotes overdue closed cycles and inserts in-app notifications.</summary>
    public async Task Execute(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var cycleIds = await _db.FinBillingCycles
            .AsNoTracking()
            .Where(c => c.Status == BillingCycleStatus.Closed && c.PaymentDueDate < today)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var cyclesUpdated = 0;
        var notificationsCreated = 0;
        var errorsCount = 0;
        var errorDetails = new List<object>();
        var vi = CultureInfo.GetCultureInfo("vi-VN");

        foreach (var cycleId in cycleIds)
        {
            if (_db is DbContext ctx)
                ctx.ChangeTracker.Clear();

            await using var tx = await _db.Database
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                var cycle = await _db.FinBillingCycles
                    .Include(c => c.Source)
                    .Include(c => c.Smodule)
                    .ThenInclude(m => m.Space)
                    .ThenInclude(s => s.Org)
                    .FirstAsync(c => c.Id == cycleId, cancellationToken)
                    .ConfigureAwait(false);

                if (cycle.Status != BillingCycleStatus.Closed)
                {
                    await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }

                var daysOverdue = today.DayNumber - cycle.PaymentDueDate.DayNumber;
                cycle.Status = BillingCycleStatus.Overdue;

                var ownerId = cycle.Smodule.Space.Org.OwnerId;
                var periodStart = cycle.PeriodStart.ToString("dd/MM/yyyy", vi);
                var periodEnd = cycle.PeriodEnd.ToString("dd/MM/yyyy", vi);
                var title = "Kỳ sao kê quá hạn";
                var body =
                    $"Kỳ sao kê [{cycle.Source.Name}] từ [{periodStart}] đến [{periodEnd}] đã quá hạn thanh toán [{daysOverdue}] ngày";

                _db.Notifications.Add(
                    new Notification
                    {
                        UserId = ownerId,
                        Type = "billing_overdue",
                        Title = title,
                        Body = body,
                        IsRead = false,
                    });

                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await tx.CommitAsync(cancellationToken).ConfigureAwait(false);

                cyclesUpdated++;
                notificationsCreated++;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                errorsCount++;
                errorDetails.Add(new { cycle_id = cycleId, message = ex.Message });
                _logger.LogError(ex, "CheckOverdue failed for billing cycle {CycleId}", cycleId);
            }
        }

        if (_db is DbContext clearCtx)
            clearCtx.ChangeTracker.Clear();

        var runAt = DateTime.UtcNow;
        var payload = JsonSerializer.Serialize(
            new
            {
                run_at = runAt,
                cycles_updated = cyclesUpdated,
                notifications_created = notificationsCreated,
                errors_count = errorsCount,
                error_details = errorDetails,
            },
            JsonOptions);

        _db.SystemEventLogs.Add(
            new SystemEventLog
            {
                EventType = "hangfire.check_overdue_billing_cycles.completed",
                JobName = "CheckOverdueBillingCycles",
                Payload = payload,
                Status = "success",
                CreatedAt = runAt,
                UpdatedAt = runAt,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
