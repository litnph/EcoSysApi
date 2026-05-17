using System.Threading;
using Hangfire;
using PFP.Application.Common.Interfaces;

namespace PFP.Infrastructure.BackgroundJobs;

/// <summary>Bridges application commands to Hangfire for GDPR exports.</summary>
public sealed class HangfireDataExportJobScheduler : IDataExportJobScheduler
{
    private readonly IBackgroundJobClient _jobs;

    /// <summary>Creates the scheduler.</summary>
    public HangfireDataExportJobScheduler(IBackgroundJobClient jobs) => _jobs = jobs;

    /// <inheritdoc/>
    public void EnqueueProcessExport(Guid exportId) =>
        _jobs.Enqueue<ProcessDataExportsJob>(job => job.ProcessSingleExportAsync(exportId, CancellationToken.None));
}
