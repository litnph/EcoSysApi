using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities;

/// <summary>
/// Central record of money movement in the finance module. Maps to <c>FIN_TRANSACTIONS</c>.
/// <para>
/// <see cref="Amount"/> is stored as a positive magnitude for <see cref="TransactionType.Direct"/> and
/// <see cref="TransactionType.Income"/>; balance direction is implied by <see cref="Type"/>.
/// For <see cref="TransactionType.Transfer"/>, the outbound leg (money leaving a source) uses a negative
/// <see cref="Amount"/> and the inbound leg uses a positive amount of the same absolute value.
/// </para>
/// </summary>
public sealed class FinTransaction : VersionedEntity
{
    /// <summary>Functional transaction kind.</summary>
    public TransactionType Type { get; set; }

    /// <summary>Workflow status (new → installment / month close).</summary>
    public TxnStatus Status { get; set; } = TxnStatus.New;

    /// <summary>
    /// Monetary amount in <see cref="Currency"/>; sign rules depend on <see cref="Type"/>
    /// (see class summary).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>ISO 4217 currency code.</summary>
    public string Currency { get; set; } = "VND";

    /// <summary>Business date of the transaction (date-only).</summary>
    public DateOnly TxnDate { get; set; }

    /// <summary>FK to <see cref="FinSource"/>.</summary>
    public Guid SourceId { get; set; }

    /// <summary>FK to destination <see cref="FinSource"/> for transfers; otherwise <c>null</c>.</summary>
    public Guid? DestSourceId { get; set; }

    /// <summary>FK to <see cref="FinCategory"/>; optional for some types.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>FK to <see cref="FinMonthlyPeriod"/> when the transaction is attributed to a closed/open period.</summary>
    public Guid? MonthlyPeriodId { get; set; }

    /// <summary>Self-FK for transfer pairs, reversals, and other linked rows.</summary>
    public Guid? RefTxnId { get; set; }

    /// <summary>FK to <see cref="FinInstallmentPlan"/> when applicable.</summary>
    public Guid? InstallmentPlanId { get; set; }

    /// <summary>Short description for lists.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Optional note (max 500 in EF mapping).</summary>
    public string? Note { get; set; }

    public decimal? ExchangeRate { get; set; }

    public string? CounterpartyName { get; set; }

    public string? ExternalRef { get; set; }

    public string? Tags { get; set; }

    // ---- Navigation ----
    public FinSource Source { get; set; } = null!;

    public FinSource? DestSource { get; set; }

    public FinCategory? Category { get; set; }

    public FinMonthlyPeriod? MonthlyPeriod { get; set; }

    public ICollection<FinBillingCycleItem> BillingCycleItems { get; set; } = new List<FinBillingCycleItem>();

    public FinTransaction? RefTransaction { get; set; }

    public ICollection<FinTransaction> RelatedTransactions { get; set; } = new List<FinTransaction>();

    public FinInstallmentPlan? InstallmentPlan { get; set; }

    public ICollection<FinTxnSplit> Splits { get; set; } = new List<FinTxnSplit>();

    public ICollection<FinTransactionHistory> History { get; set; } = new List<FinTransactionHistory>();
}
