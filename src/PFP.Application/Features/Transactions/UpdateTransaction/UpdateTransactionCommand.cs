using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.UpdateTransaction;

/// <summary>
/// Updates editable metadata on an existing <c>FIN_TRANSACTION</c> — description, note, category,
/// transaction date, and monthly-period reassignment.
/// <para>
/// <c>Amount</c> may be updated; balances and linked debt/split/billing records are adjusted in the handler.
/// <c>SourceId</c>, <c>Type</c>, and <c>DestSourceId</c> remain immutable — delete and re-create to change those.
/// </para>
/// </summary>
public sealed record UpdateTransactionCommand(
    Guid TransactionId,
    Guid? CategoryId,
    DateOnly TxnDate,
    string Description,
    string? Note,
    Guid? MonthlyPeriodId,
    long? Amount) : IRequest<UpdateTransactionResponse>;

/// <summary>Wrapper around the refreshed transaction detail.</summary>
public sealed record UpdateTransactionResponse(TransactionDetailDto Transaction);
