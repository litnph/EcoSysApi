using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>Append-only debt ledger row. Maps to <c>fin_debt_transactions</c>.</summary>
public sealed class FinDebtTransaction : BaseEntity
{
    public Guid DebtRecordId { get; set; }

    public Guid? TxnId { get; set; }

    public decimal Amount { get; set; }

    public DebtTxnType Type { get; set; }

    public string? Note { get; set; }

    public DateOnly TxnDate { get; set; }

    // ---- Navigation ----

    public FinDebtRecord DebtRecord { get; set; } = null!;

    public FinTransaction? Transaction { get; set; }
}
