using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Single row in a paginated transaction list.</summary>
public sealed record TransactionListItemDto(
    Guid Id,
    Guid SmoduleId,
    TransactionType Type,
    TxnStatus Status,
    long Amount,
    string Currency,
    DateOnly TxnDate,
    Guid SourceId,
    string SourceName,
    Guid? CategoryId,
    string? CategoryName,
    string Description,
    string? Note,
    DateTime CreatedAt);

/// <summary>Paginated list envelope.</summary>
public sealed record GetTransactionsResponse(
    IReadOnlyList<TransactionListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

/// <summary>Embedded source summary on transaction detail.</summary>
public sealed record TransactionSourceSummaryDto(Guid Id, string Name, string Currency, long Balance);

/// <summary>Embedded category summary on transaction detail.</summary>
public sealed record TransactionCategorySummaryDto(Guid Id, string Name, CategoryKind Kind);

/// <summary>Full transaction detail for GET-by-id.</summary>
public sealed record TransactionDetailDto(
    Guid Id,
    Guid SmoduleId,
    TransactionType Type,
    TxnStatus Status,
    long Amount,
    string Currency,
    DateOnly TxnDate,
    Guid SourceId,
    Guid? CategoryId,
    string Description,
    string? Note,
    Guid? BillingCycleId,
    Guid? MonthlyPeriodId,
    Guid? RefTxnId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Version,
    TransactionSourceSummaryDto? Source,
    TransactionCategorySummaryDto? Category);
