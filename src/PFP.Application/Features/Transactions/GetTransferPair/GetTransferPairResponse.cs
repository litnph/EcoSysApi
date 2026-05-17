using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.GetTransferPair;

/// <summary>Both FIN_TRANSACTIONS rows for a single logical transfer (outbound then inbound).</summary>
public sealed record GetTransferPairResponse(TransactionDetailDto Outbound, TransactionDetailDto Inbound);
