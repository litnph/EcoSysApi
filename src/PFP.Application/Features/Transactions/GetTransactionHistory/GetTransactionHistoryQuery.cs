using MediatR;

namespace PFP.Application.Features.Transactions.GetTransactionHistory;

/// <summary>Loads version history rows for a finance transaction.</summary>
public sealed record GetTransactionHistoryQuery(Guid TransactionId) : IRequest<GetTransactionHistoryResponse>;
