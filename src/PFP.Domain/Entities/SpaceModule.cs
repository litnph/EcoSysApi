using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Activation record of a pluggable module inside a <see cref="Space"/>.
/// Maps to <c>SPACE_MODULES</c>.
/// <para>
/// Per spec §3.6 every finance entity (<c>FIN_*</c>) carries a FK <c>smodule_id → SPACE_MODULES.Id</c>
/// — finance rows must NEVER reference <see cref="Space"/> or <see cref="Organization"/> directly.
/// This indirection is what lets each space toggle modules independently and what allows the
/// platform to grow new modules without touching existing finance schemas.
/// </para>
/// <para>
/// A composite unique constraint on (<see cref="SpaceId"/>, <see cref="ModuleCode"/>) prevents
/// activating the same module twice on a single space. Disabling a module sets
/// <see cref="IsEnabled"/> to <c>false</c> rather than soft-deleting the row, so historical
/// finance rows attached to <c>Id</c> remain valid.
/// </para>
/// </summary>
public sealed class SpaceModule : SoftDeletableEntity
{
    /// <summary>FK to <see cref="Space"/>.</summary>
    public Guid SpaceId { get; set; }

    /// <summary>Identifier of the module hosted in this row (e.g. <see cref="Enums.ModuleCode.Finance"/>).</summary>
    public ModuleCode ModuleCode { get; set; }

    /// <summary>Activation flag; an existing row with <c>IsEnabled = false</c> still anchors historical FKs.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Module-specific settings serialised as JSON (will be stored as <c>jsonb</c> in PostgreSQL,
    /// configured in the EF mapping). Free-form per module — the schema is owned by the consuming module.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>UTC timestamp at which the module was activated for this space.</summary>
    public DateTime EnabledAt { get; set; }

    /// <summary>UTC timestamp at which the module was last disabled; <c>null</c> while active.</summary>
    public DateTime? DisabledAt { get; set; }

    // ---- Navigation ----

    public Space Space { get; set; } = null!;

    // ---- Navigation: Layer 2 (Finance) ----

    /// <summary>All finance sources owned by this space-module (cash, bank accounts, credit cards, …).</summary>
    public ICollection<FinSource> FinSources { get; set; } = new List<FinSource>();

    /// <summary>All finance categories defined in this space-module.</summary>
    public ICollection<FinCategory> FinCategories { get; set; } = new List<FinCategory>();

    /// <summary>All finance transactions recorded in this space-module.</summary>
    public ICollection<FinTransaction> FinTransactions { get; set; } = new List<FinTransaction>();

    /// <summary>All credit-card billing cycles in this space-module.</summary>
    public ICollection<FinBillingCycle> FinBillingCycles { get; set; } = new List<FinBillingCycle>();

    /// <summary>Monthly closing periods for this space-module.</summary>
    public ICollection<FinMonthlyPeriod> FinMonthlyPeriods { get; set; } = new List<FinMonthlyPeriod>();

    /// <summary>Installment plans active or completed in this space-module.</summary>
    public ICollection<FinInstallmentPlan> FinInstallmentPlans { get; set; } = new List<FinInstallmentPlan>();

    /// <summary>Debt / loan records tracked in this space-module.</summary>
    public ICollection<FinDebtRecord> FinDebtRecords { get; set; } = new List<FinDebtRecord>();

    /// <summary>Savings books tracked in this space-module.</summary>
    public ICollection<FinSaving> FinSavings { get; set; } = new List<FinSaving>();

    /// <summary>Investment positions tracked in this space-module.</summary>
    public ICollection<FinInvestment> FinInvestments { get; set; } = new List<FinInvestment>();

    /// <summary>Finance automation rules for this module.</summary>
    public ICollection<AutomationRule> AutomationRules { get; set; } = new List<AutomationRule>();
}
