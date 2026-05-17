using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// Append-only investment ledger row. Maps to <c>FIN_INVESTMENT_TXN</c>.
/// </summary>
public sealed class FinInvestmentTxn : BaseEntity
{
    /// <summary>FK to <see cref="FinInvestment"/>.</summary>
    public Guid InvestmentId { get; set; }

    public InvestmentTxnType TxnType { get; set; }

    /// <summary>Cash magnitude for this event (always positive; semantics depend on <see cref="TxnType"/>).</summary>
    public decimal Amount { get; set; }

    /// <summary>Optional units (shares, fund units, coin quantity, …).</summary>
    public decimal? Quantity { get; set; }

    public decimal? PricePerUnit { get; set; }

    public DateOnly TxnDate { get; set; }

    public string? Note { get; set; }

    /// <summary>Optional FK to a cash-leg <see cref="FinTransaction"/>.</summary>
    public Guid? LinkedTxnId { get; set; }

    // ---- Navigation ----

    public FinInvestment Investment { get; set; } = null!;

    public FinTransaction? LinkedTxn { get; set; }
}
