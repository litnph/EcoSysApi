using MediatR;
using PFP.Application.Features.Transactions.Splits.Common;

namespace PFP.Application.Features.Transactions.Splits.SettleSplit;

/// <summary>Records reimbursement for one pending split as income into a chosen source.</summary>
public sealed record SettleSplitCommand(Guid SplitId, Guid PaymentSourceId, long? Amount) : IRequest<SettleSplitResponse>;

/// <summary>Result of settling a split line.</summary>
public sealed record SettleSplitResponse(Guid IncomeTransactionId, TxnSplitDto Split);
