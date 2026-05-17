using PFP.Application.Features.Transactions.Splits.Common;

namespace PFP.Application.Features.Transactions.Splits.GetSplitsByTransaction;

/// <summary>Split lines for one parent transaction.</summary>
public sealed record GetSplitsByTransactionResponse(IReadOnlyList<TxnSplitDto> Splits);
