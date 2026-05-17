using MediatR;

namespace PFP.Application.Features.Transactions.Splits.GetSplitsByTransaction;

/// <summary>Returns split rows for a finance transaction.</summary>
public sealed record GetSplitsByTransactionQuery(Guid TransactionId) : IRequest<GetSplitsByTransactionResponse>;
