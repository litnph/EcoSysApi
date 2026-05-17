namespace PFP.Application.Common.Interfaces;

/// <summary>Infrastructure hook to enqueue GDPR data-export processing (Hangfire).</summary>
public interface IDataExportJobScheduler
{
    /// <summary>Schedules near-immediate processing of a single export row.</summary>
    void EnqueueProcessExport(Guid exportId);
}
