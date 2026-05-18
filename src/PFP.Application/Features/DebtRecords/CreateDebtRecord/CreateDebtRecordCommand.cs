using MediatR;
using PFP.Application.Features.DebtRecords.GetDebtRecordDetail;
using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.CreateDebtRecord;

/// <summary>
/// Manually creates a <c>FIN_DEBT_RECORDS</c> row WITHOUT generating an originating
/// <c>FIN_TRANSACTION</c>. Use this when migrating pre-existing debt where the cash already moved
/// outside of the platform.
/// <para>
/// To record a debt with a money movement use <c>CreateTransactionCommand</c> with
/// <see cref="TransactionType.DebtBorrow"/> or <see cref="TransactionType.LoanGive"/>; that flow
/// generates both the transaction and the debt record in one DB transaction.
/// </para>
/// </summary>
public sealed record CreateDebtRecordCommand(
    Guid SmoduleId,
    DebtDirection Direction,
    string PersonName,
    string? PersonContact,
    long Amount,
    string? Currency,
    DateOnly? DueDate,
    string? Note) : IRequest<CreateDebtRecordResponse>;

/// <summary>Wrapper around the persisted debt record detail.</summary>
public sealed record CreateDebtRecordResponse(DebtRecordDetailDto Record);
