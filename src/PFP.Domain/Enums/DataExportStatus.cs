namespace PFP.Domain.Enums;

/// <summary>
/// State machine for <c>USER_DATA_EXPORTS.status</c> — drives the GDPR data export workflow.
/// <para>
/// Allowed transitions: <see cref="Pending"/> → <see cref="Processing"/> → <see cref="Ready"/> → <see cref="Expired"/>;
/// <see cref="Pending"/>/<see cref="Processing"/> may move to <see cref="Failed"/> on worker error.
/// The transition to <see cref="Expired"/> is performed by the <c>ProcessDataExports</c> background job
/// once the download URL passes its TTL.
/// </para>
/// </summary>
public enum DataExportStatus
{
    /// <summary><c>pending</c> — request queued, not yet picked up by the worker.</summary>
    Pending = 1,

    /// <summary><c>processing</c> — worker is collecting data and writing the export archive.</summary>
    Processing = 2,

    /// <summary><c>ready</c> — archive uploaded to storage, signed download URL valid until <c>expires_at</c>.</summary>
    Ready = 3,

    /// <summary><c>expired</c> — download window elapsed; archive may have been purged.</summary>
    Expired = 4,

    /// <summary><c>failed</c> — worker error; see <c>error_message</c> on the export row.</summary>
    Failed = 5,
}
