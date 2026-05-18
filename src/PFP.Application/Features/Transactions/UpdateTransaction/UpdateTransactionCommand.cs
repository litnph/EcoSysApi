using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.Transactions.UpdateTransaction;

/// <summary>
/// Updates editable metadata on an existing <c>FIN_TRANSACTION</c> — description, note, category,
/// transaction date, and monthly-period reassignment.
/// <para>
/// To preserve the balance / audit invariant (spec §4.6), this command intentionally does NOT
/// expose <c>Amount</c>, <c>SourceId</c>, <c>Type</c>, or <c>DestSourceId</c>. To change any of those,
/// soft-delete the transaction (which emits a reversal row and reverts balances) and re-create.
/// </para>
/// </summary>
public sealed record UpdateTransactionCommand(
    Guid TransactionId,
    Guid? CategoryId,
    DateOnly TxnDate,
    string Description,
    string? Note,
    Guid? MonthlyPeriodId) : IRequest<UpdateTransactionResponse>;

/// <summary>Wrapper around the refreshed transaction detail.</summary>
public sealed record UpdateTransactionResponse(TransactionDetailDto Transaction);
