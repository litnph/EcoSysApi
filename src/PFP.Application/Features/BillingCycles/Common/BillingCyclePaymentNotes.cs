namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Canonical copy used when posting a statement payment transaction.</summary>
public static class BillingCyclePaymentNotes
{
    /// <summary>Vietnamese note attached to <see cref="PFP.Domain.Enums.TransactionType.Direct"/> payments toward a billing cycle.</summary>
    public const string StatementPayment = "Thanh toán kỳ sao kê";
}
