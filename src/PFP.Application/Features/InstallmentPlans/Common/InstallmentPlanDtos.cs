using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>List row for <see cref="GetInstallmentPlans.GetInstallmentPlansQuery"/>.</summary>
public sealed record InstallmentPlanListItemDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string? SourceIcon,
    string? SourceColor,
    string OriginalTxnDescription,
    string? OriginalTxnCategoryName,
    InstallmentStatus Status,
    int PaidInstallments,
    int TotalInstallments,
    long RemainingAmount,
    long TotalAmount,
    bool CanDelete,
    DateTime CreatedAt);

/// <summary>Detail DTO for a single pay line.</summary>
public sealed record InstallmentPayItemDto(
    int InstallmentNumber,
    DateOnly DueDate,
    long Amount,
    long PaidAmount,
    InstallmentPayStatus Status,
    DateTime? PaidAt,
    Guid? TxnId);

/// <summary>Full plan detail for <see cref="GetInstallmentPlanDetail.GetInstallmentPlanDetailQuery"/>.</summary>
public sealed record InstallmentPlanDetailDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string? SourceIcon,
    string? SourceColor,
    Guid OriginalTxnId,
    string OriginalTxnDescription,
    string? OriginalTxnCategoryName,
    long TotalAmount,
    int TotalMonths,
    long MonthlyAmount,
    decimal InterestRate,
    decimal? ConversionFeeRate,
    long? ConversionFeeAmount,
    ConversionFeeStatus? ConversionFeeStatus,
    Guid? ConversionFeeTxnId,
    DateOnly StartDate,
    InstallmentStatus Status,
    string? CancellationReason,
    bool CanDelete,
    IReadOnlyList<InstallmentPayItemDto> Pays);
