using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed record DebtRecordListItemDto(
    Guid Id,
    DebtDirection Direction,
    string PersonName,
    string? PersonContact,
    long OriginalAmount,
    long RemainingAmount,
    string Currency,
    DateOnly? DueDate,
    DebtStatus Status,
    int? DaysUntilDue,
    DateTime CreatedAt);

public sealed record GetDebtRecordsResponse(IReadOnlyList<DebtRecordListItemDto> Items);
