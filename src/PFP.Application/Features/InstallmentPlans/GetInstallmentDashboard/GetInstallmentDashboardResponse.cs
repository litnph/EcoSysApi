using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.GetInstallmentDashboard;

public sealed record GetInstallmentDashboardResponse(InstallmentDashboardDto Dashboard);

public sealed record InstallmentDashboardDto(
    int ActivePlanCount,
    long TotalRemainingAmount,
    int DueCount,
    long DueAmount,
    int OverdueCount,
    long OverdueAmount,
    int UpcomingCount,
    long UpcomingAmount,
    int ThisMonthDueCount,
    long ThisMonthDueAmount,
    int NextMonthDueCount,
    long NextMonthDueAmount,
    int CompletionPercent,
    IReadOnlyList<InstallmentDashboardSourceDto> BySource,
    IReadOnlyList<InstallmentUpcomingPayDto> UpcomingPays,
    IReadOnlyList<InstallmentSchedulePayDto> SchedulePays);

public sealed record InstallmentDashboardSourceDto(
    Guid SourceId,
    string SourceName,
    string? SourceIcon,
    string? SourceColor,
    int ActivePlanCount,
    long RemainingAmount,
    long OverdueAmount,
    long ThisMonthDueAmount,
    long NextMonthDueAmount);

public sealed record InstallmentUpcomingPayDto(
    Guid PlanId,
    Guid SourceId,
    string SourceName,
    string? SourceIcon,
    string PlanTitle,
    int InstallmentNumber,
    int TotalInstallments,
    DateOnly StatementDate,
    DateOnly DueDate,
    long Amount,
    InstallmentUpcomingPayBucket Bucket);

public sealed record InstallmentSchedulePayDto(
    Guid PlanId,
    Guid SourceId,
    string SourceName,
    string? SourceIcon,
    string PlanTitle,
    int InstallmentNumber,
    int TotalInstallments,
    DateOnly StatementDate,
    DateOnly DueDate,
    long Amount,
    InstallmentPayStatus Status,
    DateTime? PaidAt,
    InstallmentUpcomingPayBucket? Bucket);

public enum InstallmentUpcomingPayBucket
{
    Overdue = 1,
    DueToday = 2,
    ThisMonth = 3,
    NextMonth = 4,
    Later = 5,
}
