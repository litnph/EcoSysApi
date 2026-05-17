using MediatR;

namespace PFP.Application.Features.Transactions.DeleteTransaction;

/// <summary>Soft-deletes a finance transaction and posts the compensating reversal row(s) per spec §4.2.</summary>
public sealed record DeleteTransactionCommand(Guid TransactionId, string? Reason) : IRequest<DeleteTransactionResponse>;
