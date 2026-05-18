using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecordDetail;

public sealed record DebtTransactionItemDto(
    Guid Id,
    Guid? TxnId,
    long Amount,
    DebtTxnType Type,
    string? Note,
    DateOnly TxnDate,
    DateTime CreatedAt);

public sealed record DebtRecordDetailDto(
    Guid Id,
    Guid SmoduleId,
    DebtDirection Direction,
    string PersonName,
    string? PersonContact,
    Guid? OriginalTxnId,
    long OriginalAmount,
    long RemainingAmount,
    string Currency,
    DateOnly? DueDate,
    DebtStatus Status,
    string? Note,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<DebtTransactionItemDto> Transactions);

public sealed record GetDebtRecordDetailResponse(DebtRecordDetailDto Record);
