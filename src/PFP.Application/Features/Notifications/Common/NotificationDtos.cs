using PFP.Application.Common;
using PFP.Domain.Entities;
using PFP.Domain.Enums;

namespace PFP.Application.Features.Notifications.Common;

public sealed record NotificationDto(
    Guid Id,
    NotificationKind Kind,
    Guid? CategoryId,
    string? CategoryName,
    string? Currency,
    long? SpentAmount,
    long? BudgetAmount,
    decimal? UtilizationPercent,
    decimal? ThresholdPercent,
    DateOnly? CycleStart,
    DateOnly? CycleEnd,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(UserNotification row) => new(
        row.Id,
        row.Kind,
        row.CategoryId,
        row.CategoryName,
        row.Currency,
        row.SpentAmount.HasValue ? CurrencyUnits.ToWhole(row.SpentAmount.Value) : null,
        row.BudgetAmount.HasValue ? CurrencyUnits.ToWhole(row.BudgetAmount.Value) : null,
        row.UtilizationPercent,
        row.ThresholdPercent,
        row.CycleStart,
        row.CycleEnd,
        row.IsRead,
        row.ReadAt,
        row.CreatedAt);
}
