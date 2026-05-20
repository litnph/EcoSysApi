using Microsoft.Extensions.Logging;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.Gdpr;

/// <summary>
/// Placeholder scheduler after Hangfire removal. Export rows stay <c>Pending</c> until a worker is added.
/// </summary>
public sealed class NoOpDataExportJobScheduler : IDataExportJobScheduler
{
    private readonly ILogger<NoOpDataExportJobScheduler> _logger;

    public NoOpDataExportJobScheduler(ILogger<NoOpDataExportJobScheduler> logger) => _logger = logger;

    public void EnqueueProcessExport(Guid exportId) =>
        _logger.LogInformation(
            "Data export {ExportId} queued — background processing is not configured.",
            exportId);
}
