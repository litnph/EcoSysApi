namespace PFP.API.Models;

/// <summary>JSON body for <c>POST /api/v1/finance/splits/{{id}}/settle</c>.</summary>
public sealed class SettleSplitRequest
{
    /// <summary>Source that receives the reimbursement (balance increases).</summary>
    public Guid PaymentSourceId { get; set; }

    /// <summary>Optional override; defaults to the full split amount for this participant.</summary>
    public long? Amount { get; set; }
}
