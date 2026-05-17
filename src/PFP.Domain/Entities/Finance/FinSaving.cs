using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>
/// User-managed savings goal / deposit book. Maps to <c>FIN_SAVINGS</c>.
/// </summary>
public sealed class FinSaving : SoftDeletableEntity
{
    /// <summary>FK to <see cref="SpaceModule"/> (finance module activation).</summary>
    public Guid SmoduleId { get; set; }

    /// <summary>FK to the liquid <see cref="FinSource"/> linked to this savings record.</summary>
    public Guid SourceId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Optional savings target.</summary>
    public decimal? TargetAmount { get; set; }

    /// <summary>Current accumulated balance for this savings record.</summary>
    public decimal CurrentAmount { get; set; }

    /// <summary>Annual interest rate (percentage points), e.g. <c>6.5000</c> for 6.5%.</summary>
    public decimal InterestRate { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? MaturityDate { get; set; }

    public SavingType Type { get; set; }

    public SavingStatus Status { get; set; } = SavingStatus.Active;

    public string? Note { get; set; }

    // ---- Navigation ----

    public SpaceModule Smodule { get; set; } = null!;

    public FinSource Source { get; set; } = null!;
}
