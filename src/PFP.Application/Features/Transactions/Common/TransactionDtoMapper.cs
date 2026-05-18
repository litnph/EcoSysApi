using PFP.Application.Common;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Maps <see cref="FinTransaction"/> aggregates to API DTOs (amounts as whole currency units).</summary>
public static class TransactionDtoMapper
{
    /// <summary>Builds a list row from a transaction entity.</summary>
    public static TransactionListItemDto ToListItem(
        FinTransaction t,
        string sourceName,
        string? categoryName) =>
        new(
            t.Id,
            t.SmoduleId,
            t.Type,
            t.Status,
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

    /// <summary>Builds an embedded source summary.</summary>
    public static TransactionSourceSummaryDto ToSourceSummary(FinSource source) =>
        new(source.Id, source.Name, source.Currency, CurrencyUnits.ToWhole(source.Balance));

    /// <summary>Builds full transaction detail.</summary>
    public static TransactionDetailDto ToDetail(FinTransaction t)
    {
        TransactionSourceSummaryDto? src = t.Source is null ? null : ToSourceSummary(t.Source);
        TransactionCategorySummaryDto? cat = t.Category is null
            ? null
            : new TransactionCategorySummaryDto(t.Category.Id, t.Category.Name, t.Category.Kind);

        return new TransactionDetailDto(
            t.Id,
            t.SmoduleId,
            t.Type,
            t.Status,
            CurrencyUnits.ToWhole(t.Amount),
            t.Currency,
            t.TxnDate,
            t.SourceId,
            t.CategoryId,
            t.Description,
            t.Note,
            t.BillingCycleId,
            t.MonthlyPeriodId,
            t.RefTxnId,
            t.CreatedAt,
            t.UpdatedAt,
            t.Version,
            src,
            cat);
    }
}
