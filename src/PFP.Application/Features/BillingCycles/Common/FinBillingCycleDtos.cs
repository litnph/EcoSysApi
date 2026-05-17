using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>List row for a billing cycle including the parent source display name.</summary>
public sealed record FinBillingCycleDto(
    Guid Id,
    Guid SmoduleId,
    Guid SourceId,
    string SourceName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly StatementDate,
    DateOnly PaymentDueDate,
    decimal TotalAmount,
    decimal PaidAmount,
    BillingCycleStatus Status,
    DateTime? ClosedAt,
    DateTime? PaidAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Single transaction line shown on the billing-cycle detail screen.</summary>
public sealed record FinBillingCycleTransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Currency,
    DateOnly TxnDate,
    Guid SourceId,
    string SourceName,
    Guid? CategoryId,
    string? CategoryName,
    string Description,
    string? Note,
    DateTime CreatedAt);

/// <summary>Cycle header plus ordered activity for the detail endpoint.</summary>
public sealed record FinBillingCycleDetailDto(
    FinBillingCycleDto Cycle,
    IReadOnlyList<FinBillingCycleTransactionDto> Transactions);
