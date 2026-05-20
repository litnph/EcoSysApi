using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>Debt / loan relationship row. Maps to <c>fin_debt_records</c>.</summary>
public sealed class FinDebtRecord : VersionedEntity
{
    public DebtDirection Direction { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public string? PersonContact { get; set; }

    public Guid? OriginalTxnId { get; set; }

    public decimal OriginalAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public string Currency { get; set; } = "VND";

    public DateOnly? DueDate { get; set; }

    public DebtStatus Status { get; set; } = DebtStatus.Active;

    public string? Note { get; set; }

    // ---- Navigation ----
    public FinTransaction? OriginalTransaction { get; set; }

    public ICollection<FinDebtTransaction> FinDebtTransactions { get; set; } = new List<FinDebtTransaction>();

    public ICollection<FinDebtRecordHistory> History { get; set; } = new List<FinDebtRecordHistory>();
}
