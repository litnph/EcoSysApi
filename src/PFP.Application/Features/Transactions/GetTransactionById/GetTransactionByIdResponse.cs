using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.GetTransactionById;

/// <summary>Detail payload for <see cref="GetTransactionByIdQuery"/>.</summary>
public sealed record GetTransactionByIdResponse(TransactionDetailDto Transaction);
