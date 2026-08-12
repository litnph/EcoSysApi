using PFP.Domain.Enums;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Tag linked to a transaction via <c>entity_tags</c>.</summary>
public sealed record TransactionTagDto(Guid Id, string Name, string Color);

/// <summary>
/// Single row in a paginated transaction list. Billing-cycle statement month uses YYYY-MM when active.
/// </summary>
public sealed record TransactionListItemDto(
    Guid Id,
    TransactionType Type,
    TransactionPurpose Purpose,
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
    DateTime CreatedAt,
    bool HasInstallmentPlan,
    bool IsInstallmentPayment,
    string? BillingCycleStatementMonth,
    IReadOnlyList<TransactionTagDto> Tags);

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
    TransactionType Type,
    TransactionPurpose Purpose,
    TxnStatus Status,
    long Amount,
    string Currency,
    DateOnly TxnDate,
    Guid SourceId,
    Guid? CategoryId,
    string Description,
    string? Note,
    Guid? MonthlyPeriodId,
    Guid? RefTxnId,
    Guid? SavingId,
    Guid? BillingCycleId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int Version,
    bool CanEditAmount,
    bool CanDelete,
    bool HasInstallmentPlan,
    bool IsInstallmentPayment,
    TransactionSourceSummaryDto? Source,
    TransactionCategorySummaryDto? Category,
    IReadOnlyList<TransactionTagDto> Tags);
