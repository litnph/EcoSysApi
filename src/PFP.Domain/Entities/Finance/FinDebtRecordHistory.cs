using PFP.Domain.Entities;

namespace PFP.Domain.Entities.Finance;

/// <summary>Version history for <see cref="FinDebtRecord"/>. Maps to <c>fin_debt_record_history</c>.</summary>
public sealed class FinDebtRecordHistory : VersionHistoryEntity
{
    public Guid EntityId { get; set; }

    public FinDebtRecord Entity { get; set; } = null!;
}
