using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>Installment conversion plan for a deferred credit-card transaction. Maps to <c>fin_installment_plans</c>.</summary>
public sealed class FinInstallmentPlan : VersionedEntity
{
    /// <summary>FK to the original <see cref="FinTransaction"/> (type <see cref="TransactionType.Deferred"/>).</summary>
    public Guid OriginalTxnId { get; set; }

    public Guid SourceId { get; set; }

    public decimal TotalAmount { get; set; }

    public int TotalMonths { get; set; }

    public decimal MonthlyAmount { get; set; }

    /// <summary>Nominal interest rate percent (e.g. <c>0</c> for 0% campaign).</summary>
    public decimal InterestRate { get; set; }

    public decimal? ConversionFeeRate { get; set; }

    public decimal? ConversionFeeAmount { get; set; }

    public ConversionFeeStatus? ConversionFeeStatus { get; set; }

    public Guid? ConversionFeeTxnId { get; set; }

    public DateOnly StartDate { get; set; }

    public InstallmentStatus Status { get; set; } = InstallmentStatus.Active;

    /// <summary>Optional reason when <see cref="Status"/> becomes <see cref="InstallmentStatus.Cancelled"/>.</summary>
    public string? CancellationReason { get; set; }
    public FinSource Source { get; set; } = null!;

    public FinTransaction OriginalTransaction { get; set; } = null!;

    public FinTransaction? ConversionFeeTxn { get; set; }

    public ICollection<FinInstallmentPay> Pays { get; set; } = new List<FinInstallmentPay>();

    public ICollection<FinInstallmentPlanHistory> History { get; set; } = new List<FinInstallmentPlanHistory>();
}
