namespace PFP.Application.Features.Transactions.DeleteTransaction;

/// <summary>Payload returned after <see cref="DeleteTransactionCommand"/>.</summary>
public sealed record DeleteTransactionResponse(Guid TransactionId);
