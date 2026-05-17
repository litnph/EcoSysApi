using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// A user-owned source of funds: cash, bank account, credit card, e-wallet, or investment account.
/// Maps to <c>FIN_SOURCES</c>.
/// <para>
/// Per spec §3.6 every finance row references the space indirectly through
/// <see cref="SmoduleId"/> → <see cref="SpaceModule"/> — never through <see cref="Space"/> or
/// <see cref="Organization"/> directly. This indirection lets each space toggle the finance
/// module independently while preserving the FK graph.
/// </para>
/// <para>
/// <b>Balance is sacrosanct.</b> Per spec §4.6 the <see cref="Balance"/> column is NEVER updated
/// in place. It moves only through (a) the creation of a <see cref="FinTransaction"/>, (b) a
/// reversal row emitted by the soft-delete flow, or (c) a reconciliation pass by
/// <c>IBalanceCalculator.RecalculateBalance</c>. Repositories MUST NOT expose a setter shortcut.
/// </para>
/// <para>
/// Credit-card-specific columns (<see cref="CreditLimit"/>, <see cref="StatementDay"/>,
/// <see cref="PaymentDueDay"/>, <see cref="MinInstallmentAmt"/>) are nullable and only populated
/// when <see cref="Type"/> = <see cref="SourceType.CreditCard"/>. For credit cards
/// <see cref="Balance"/> stores the outstanding (unpaid) amount.
/// </para>
/// <para>
/// Versioned: every update appends to <c>FIN_SOURCES_HISTORY</c> via the history interceptor
/// (see <see cref="FinSourceHistory"/>).
/// </para>
/// </summary>
public sealed class FinSource : VersionedEntity
{
    /// <summary>FK to <see cref="SpaceModule"/> (the row that activates the finance module on a space).</summary>
    public Guid SmoduleId { get; set; }

    /// <summary>Display name shown in the source picker / dashboard.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Type of source — drives which extra columns are required / displayed.</summary>
    public SourceType Type { get; set; }

    /// <summary>
    /// Current balance. For asset sources (cash / bank / e-wallet / investment) this is the
    /// amount available; for <see cref="SourceType.CreditCard"/> this is the outstanding debt.
    /// <para>NEVER write to this column directly — see spec §4.6.</para>
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>ISO 4217 currency code (e.g. <c>VND</c>, <c>USD</c>). Defaults to the organisation's <c>default_currency</c>.</summary>
    public string Currency { get; set; } = "VND";

    /// <summary>Credit limit. Required when <see cref="Type"/> = <see cref="SourceType.CreditCard"/>, otherwise <c>null</c>.</summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>Day-of-month on which the credit-card statement is issued (1–31). Credit cards only.</summary>
    public int? StatementDay { get; set; }

    /// <summary>Number of days after <see cref="StatementDay"/> by which the cycle must be paid. Credit cards only.</summary>
    public int? PaymentDueDay { get; set; }

    /// <summary>
    /// Minimum single-transaction amount eligible for conversion into an installment plan
    /// (per spec §4.3 validation step 1). Optional; if <c>null</c>, the platform default applies.
    /// </summary>
    public decimal? MinInstallmentAmt { get; set; }

    /// <summary>Optional opening / starter balance recorded when the source was first created (for reporting).</summary>
    public decimal? InitialBalance { get; set; }

    /// <summary>Optional icon key (front-end pick-list) shown next to the source name.</summary>
    public string? Icon { get; set; }

    /// <summary>Optional accent colour (hex, e.g. <c>#1F6FEB</c>) for charts and badges.</summary>
    public string? Color { get; set; }

    /// <summary>Free-form description / notes about the source.</summary>
    public string? Description { get; set; }

    /// <summary>UI hint: ascending sort order in the source picker. Default <c>0</c>.</summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// <c>true</c> while the source is hidden from default lists but not yet soft-deleted.
    /// Archived sources still anchor historical FKs.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>Optional external identifier used by bank-sync providers (Plaid, Tink, …).</summary>
    public string? ExternalRef { get; set; }

    // ---- Navigation ----

    public SpaceModule Smodule { get; set; } = null!;

    /// <summary>All transactions whose <c>SourceId</c> points at this source (outgoing side for transfers).</summary>
    public ICollection<FinTransaction> Transactions { get; set; } = new List<FinTransaction>();

    /// <summary>All billing cycles attached to this source; non-empty only for credit cards.</summary>
    public ICollection<FinBillingCycle> BillingCycles { get; set; } = new List<FinBillingCycle>();

    /// <summary>All installment plans whose principal lives on this card.</summary>
    public ICollection<FinInstallmentPlan> InstallmentPlans { get; set; } = new List<FinInstallmentPlan>();

    /// <summary>Version history rows for this source (append-only).</summary>
    public ICollection<FinSourceHistory> History { get; set; } = new List<FinSourceHistory>();
}
