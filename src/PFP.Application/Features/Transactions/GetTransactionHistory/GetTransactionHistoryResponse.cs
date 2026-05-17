using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.GetTransactionHistory;

/// <summary>One row from <c>fin_transaction_history</c> for API responses.</summary>
public sealed record TransactionHistoryItemDto(
    Guid Id,
    Guid TransactionId,
    int Version,
    Guid? ChangedBy,
    Guid? SessionId,
    HistoryChangeType ChangeType,
    string? ChangedFields,
    string? Snapshot,
    string? ChangeReason,
    DateTime CreatedAt);

/// <summary>Ordered history list for <see cref="GetTransactionHistoryQuery"/>.</summary>
public sealed record GetTransactionHistoryResponse(IReadOnlyList<TransactionHistoryItemDto> Items);
