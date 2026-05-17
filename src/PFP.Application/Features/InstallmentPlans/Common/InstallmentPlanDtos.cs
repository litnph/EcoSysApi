using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>List row for <see cref="GetInstallmentPlans.GetInstallmentPlansQuery"/>.</summary>
public sealed record InstallmentPlanListItemDto(
    Guid Id,
    Guid SmoduleId,
    Guid SourceId,
    string SourceName,
    string OriginalTxnDescription,
    InstallmentStatus Status,
    int PaidInstallments,
    int TotalInstallments,
    decimal RemainingAmount,
    DateTime CreatedAt);

/// <summary>Detail DTO for a single pay line.</summary>
public sealed record InstallmentPayItemDto(
    int InstallmentNumber,
    DateOnly DueDate,
    decimal Amount,
    decimal PaidAmount,
    InstallmentPayStatus Status,
    DateTime? PaidAt,
    Guid? TxnId);

/// <summary>Full plan detail for <see cref="GetInstallmentPlanDetail.GetInstallmentPlanDetailQuery"/>.</summary>
public sealed record InstallmentPlanDetailDto(
    Guid Id,
    Guid SmoduleId,
    Guid SourceId,
    string SourceName,
    Guid OriginalTxnId,
    string OriginalTxnDescription,
    decimal TotalAmount,
    int TotalMonths,
    decimal MonthlyAmount,
    decimal InterestRate,
    decimal? ConversionFeeRate,
    decimal? ConversionFeeAmount,
    ConversionFeeStatus? ConversionFeeStatus,
    Guid? ConversionFeeTxnId,
    DateOnly StartDate,
    InstallmentStatus Status,
    string? CancellationReason,
    IReadOnlyList<InstallmentPayItemDto> Pays);
