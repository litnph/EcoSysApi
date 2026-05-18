using MediatR;
using PFP.Application.Features.Transactions.Common;

namespace PFP.Application.Features.DebtRecords.RecordDebtPayment;

/// <summary>
/// Records a payment against an existing <c>FIN_DEBT_RECORDS</c> row by dispatching the
/// appropriate <c>CreateTransactionCommand</c> internally (<c>debt_repay</c> for borrowed money,
/// <c>loan_collect</c> for money lent out).
/// <para>
/// The cash account whose balance changes is <see cref="SourceId"/>. The <see cref="Amount"/> must
/// be a positive magnitude and is bounded by the debt's <c>RemainingAmount</c> — see spec §4.4
/// for the full settlement rules.
/// </para>
/// </summary>
public sealed record RecordDebtPaymentCommand(
    Guid DebtRecordId,
    Guid SourceId,
    long Amount,
    DateOnly TxnDate,
    string? Note) : IRequest<RecordDebtPaymentResponse>;

/// <summary>Wrapper around the freshly created repayment transaction detail.</summary>
public sealed record RecordDebtPaymentResponse(TransactionDetailDto Transaction);
