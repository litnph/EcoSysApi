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
        string? categoryName,
        bool hasInstallmentPlan = false,
        bool isInstallmentPayment = false,
        IReadOnlyList<TransactionTagDto>? tags = null) =>
        new(
            t.Id,
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
            t.CreatedAt,
            hasInstallmentPlan,
            isInstallmentPayment,
            tags ?? Array.Empty<TransactionTagDto>());

    /// <summary>Builds an embedded source summary.</summary>
    public static TransactionSourceSummaryDto ToSourceSummary(FinSource source) =>
        new(source.Id, source.Name, source.Currency, CurrencyUnits.ToWhole(source.Balance));

    /// <summary>Builds full transaction detail.</summary>
    public static TransactionDetailDto ToDetail(
        FinTransaction t,
        bool canEditAmount = true,
        bool canDelete = true,
        bool hasInstallmentPlan = false,
        bool isInstallmentPayment = false,
        IReadOnlyList<TransactionTagDto>? tags = null)
    {
        TransactionSourceSummaryDto? src = t.Source is null ? null : ToSourceSummary(t.Source);
        TransactionCategorySummaryDto? cat = t.Category is null
            ? null
            : new TransactionCategorySummaryDto(t.Category.Id, t.Category.Name, t.Category.Kind);

        return new TransactionDetailDto(
            t.Id,
            t.Type,
            t.Status,
            CurrencyUnits.ToWhole(t.Amount),
            t.Currency,
            t.TxnDate,
            t.SourceId,
            t.CategoryId,
            t.Description,
            t.Note,
            t.MonthlyPeriodId,
            t.RefTxnId,
            t.CreatedAt,
            t.UpdatedAt,
            t.Version,
            canEditAmount,
            canDelete,
            hasInstallmentPlan,
            isInstallmentPayment,
            src,
            cat,
            tags ?? Array.Empty<TransactionTagDto>());
    }
}
