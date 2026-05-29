using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.BillingCycles.GetBillingCycleAddableTransactions;

public sealed record GetBillingCycleAddableTransactionsQuery(Guid CycleId)
    : IRequest<GetBillingCycleAddableTransactionsResponse>;

public sealed record GetBillingCycleAddableTransactionsResponse(
    IReadOnlyList<TransactionListItemDto> Items);
