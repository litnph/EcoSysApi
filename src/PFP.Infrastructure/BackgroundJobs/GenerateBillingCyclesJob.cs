using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;
using PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Daily job: on each card's statement day, opens the next billing cycle via MediatR.</summary>
public sealed class GenerateBillingCyclesJob
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly IApplicationDbContext _db;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateBillingCyclesJob> _logger;

    /// <summary>Creates the job.</summary>
    public GenerateBillingCyclesJob(
        IApplicationDbContext db,
        IMediator mediator,
        ILogger<GenerateBillingCyclesJob> logger)
    {
        _db = db;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>Processes all eligible credit-card sources whose statement day is today (UTC).</summary>
    public async Task Execute(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayOfMonth = today.Day;

        var sources = await _db.FinSources
            .AsNoTracking()
            .Include(s => s.Smodule)
            .Where(s =>
                s.Type == SourceType.CreditCard
                && s.StatementDay == dayOfMonth
                && s.PaymentDueDay != null
                && s.Smodule.ModuleCode == ModuleCode.Finance
                && s.Smodule.IsEnabled)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var sourcesProcessed = sources.Count;
        var cyclesCreated = 0;
        var errorsCount = 0;
        var errorDetails = new List<object>();

        foreach (var source in sources)
        {
            try
            {
                await _mediator
                    .Send(new GenerateBillingCycleCommand(source.Id), cancellationToken)
                    .ConfigureAwait(false);
                cyclesCreated++;
            }
            catch (Exception ex)
            {
                errorsCount++;
                errorDetails.Add(new { source_id = source.Id, message = ex.Message });
                _logger.LogError(ex, "GenerateBillingCycle failed for source {SourceId}", source.Id);
            }
        }

        var runAt = DateTime.UtcNow;
        var payload = JsonSerializer.Serialize(
            new
            {
                run_at = runAt,
                sources_processed = sourcesProcessed,
                cycles_created = cyclesCreated,
                errors_count = errorsCount,
                error_details = errorDetails,
            },
            JsonOptions);

        _db.SystemEventLogs.Add(
            new SystemEventLog
            {
                EventType = "hangfire.generate_billing_cycles.completed",
                JobName = "GenerateBillingCycles",
                Payload = payload,
                Status = "success",
                CreatedAt = runAt,
                UpdatedAt = runAt,
            });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
