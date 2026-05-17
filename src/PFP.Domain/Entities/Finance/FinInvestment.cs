using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// Aggregated investment position. Maps to <c>FIN_INVESTMENTS</c>.
/// </summary>
public sealed class FinInvestment : SoftDeletableEntity
{
    /// <summary>FK to <see cref="SpaceModule"/>.</summary>
    public Guid SmoduleId { get; set; }

    public string Name { get; set; } = string.Empty;

    public InvestmentType Type { get; set; }

    /// <summary>Latest mark-to-market or manually maintained position value.</summary>
    public decimal CurrentValue { get; set; }

    /// <summary>Cumulative cash deployed into the position (buys + fees).</summary>
    public decimal TotalInvested { get; set; }

    /// <summary>Cumulative cash returned (dividends + sell proceeds).</summary>
    public decimal TotalReturned { get; set; }

    public string Currency { get; set; } = "VND";

    public string? Note { get; set; }

    // ---- Navigation ----

    public SpaceModule Smodule { get; set; } = null!;

    /// <summary>Append-only ledger rows.</summary>
    public ICollection<FinInvestmentTxn> InvestmentTxns { get; set; } = new List<FinInvestmentTxn>();
}
