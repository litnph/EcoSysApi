namespace PFP.Domain.Entities;

/// <summary>Append-only version row for <see cref="FinTransaction"/>. Maps to <c>fin_transaction_history</c>.</summary>
public sealed class FinTransactionHistory : VersionHistoryEntity
{
    /// <summary>FK to the parent <see cref="FinTransaction"/> (column <c>transaction_id</c>).</summary>
    public Guid TransactionId { get; set; }

    public FinTransaction Transaction { get; set; } = null!;
}
