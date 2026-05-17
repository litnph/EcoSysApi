using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Domain.Entities.Finance;

/// <summary>One scheduled installment row. Maps to <c>fin_installment_pays</c> (no soft delete).</summary>
public sealed class FinInstallmentPay : BaseEntity
{
    public Guid PlanId { get; set; }

    public int InstallmentNumber { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public InstallmentPayStatus Status { get; set; } = InstallmentPayStatus.Upcoming;

    public DateTime? PaidAt { get; set; }

    public Guid? TxnId { get; set; }

    public FinInstallmentPlan Plan { get; set; } = null!;

    public FinTransaction? Transaction { get; set; }
}
