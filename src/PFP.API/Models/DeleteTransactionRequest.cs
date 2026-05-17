namespace PFP.API.Models;

/// <summary>Optional body for DELETE /api/v1/finance/transactions/{transactionId}.</summary>
public sealed record DeleteTransactionRequest(string? Reason);
