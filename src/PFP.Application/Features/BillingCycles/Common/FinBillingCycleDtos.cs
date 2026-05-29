using PFP.Domain.Enums;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>List row for a billing cycle including the parent source display name.</summary>
public sealed record FinBillingCycleDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string Name,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly StatementDate,
    DateOnly PaymentDueDate,
    long TotalAmount,
    long PaidAmount,
    BillingCycleStatus Status,
    DateTime? ClosedAt,
    DateTime? PaidAt,
    DateTime? LastRefreshedAt,
    string? ReconciliationNote,
    long? IssuerStatementAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Single transaction line shown on the billing-cycle detail screen.</summary>
public sealed record FinBillingCycleTransactionDto(
    Guid Id,
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
    BillingCycleItemInclusionSource InclusionSource,
    DateTime CreatedAt);

/// <summary>Installment pay line due in the statement month.</summary>
public sealed record FinBillingCycleInstallmentDueDto(
    Guid PayId,
    Guid PlanId,
    Guid OriginalTxnId,
    string PlanDescription,
    string? CategoryName,
    int InstallmentNumber,
    int TotalInstallments,
    DateOnly DueDate,
    long Amount,
    long PaidAmount,
    InstallmentPayStatus Status);

/// <summary>Cycle header plus ordered activity for the detail endpoint.</summary>
public sealed record FinBillingCycleDetailDto(
    FinBillingCycleDto Cycle,
    IReadOnlyList<FinBillingCycleTransactionDto> Transactions,
    IReadOnlyList<FinBillingCycleInstallmentDueDto> InstallmentDues);
