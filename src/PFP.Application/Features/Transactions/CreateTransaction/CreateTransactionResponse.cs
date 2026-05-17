using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.CreateTransaction;

/// <summary>Payload returned after <see cref="CreateTransactionCommand"/>.</summary>
public sealed record CreateTransactionResponse(TransactionDetailDto Transaction);
