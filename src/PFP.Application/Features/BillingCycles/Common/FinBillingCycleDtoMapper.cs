using PFP.Application.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;

namespace PFP.Application.Features.BillingCycles.Common;

/// <summary>Maps billing-cycle aggregates to API DTOs.</summary>
public static class FinBillingCycleDtoMapper
{
    /// <summary>Maps a cycle entity plus display name.</summary>
    public static FinBillingCycleDto ToDto(FinBillingCycle cycle, string sourceName) =>
        new(
            cycle.Id,
            cycle.SourceId,
            sourceName,
            cycle.PeriodStart,
            cycle.PeriodEnd,
            cycle.StatementDate,
            cycle.PaymentDueDate,
            CurrencyUnits.ToWhole(cycle.TotalAmount),
            CurrencyUnits.ToWhole(cycle.PaidAmount),
            cycle.Status,
            cycle.ClosedAt,
            cycle.PaidAt,
            cycle.CreatedAt,
            cycle.UpdatedAt);

    /// <summary>Maps a transaction line on a billing-cycle statement.</summary>
    public static FinBillingCycleTransactionDto ToTransactionDto(FinTransaction t, string sourceName, string? categoryName) =>
        new(
            t.Id,
            t.Type,
            CurrencyUnits.ToWhole(t.Amount),
            t.Currency,
            t.TxnDate,
            t.SourceId,
            sourceName,
            t.CategoryId,
            categoryName,
            t.Description,
            t.Note,
            t.CreatedAt);
}
