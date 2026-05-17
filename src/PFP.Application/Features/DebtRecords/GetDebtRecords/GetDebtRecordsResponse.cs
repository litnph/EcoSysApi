using PFP.Domain.Enums;

namespace PFP.Application.Features.DebtRecords.GetDebtRecords;

public sealed record DebtRecordListItemDto(
    Guid Id,
    Guid SmoduleId,
    DebtDirection Direction,
    string PersonName,
    string? PersonContact,
    decimal OriginalAmount,
    decimal RemainingAmount,
    string Currency,
    DateOnly? DueDate,
    DebtStatus Status,
    int? DaysUntilDue,
    DateTime CreatedAt);

public sealed record GetDebtRecordsResponse(IReadOnlyList<DebtRecordListItemDto> Items);
