using PFP.Application.Common;
using PFP.Domain.Entities;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

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
            cycle.Name,
            cycle.PeriodStart,
            cycle.PeriodEnd,
            cycle.StatementDate,
            cycle.PaymentDueDate,
            CurrencyUnits.ToWhole(cycle.TotalAmount),
            CurrencyUnits.ToWhole(cycle.PaidAmount),
            cycle.Status,
            cycle.ClosedAt,
            cycle.PaidAt,
            cycle.LastRefreshedAt,
            cycle.ReconciliationNote,
            cycle.IssuerStatementAmount is { } issuer
                ? CurrencyUnits.ToWhole(issuer)
                : null,
            cycle.CreatedAt,
            cycle.UpdatedAt);

    /// <summary>Maps a transaction line on a billing-cycle statement.</summary>
    public static FinBillingCycleTransactionDto ToTransactionDto(
        FinTransaction t,
        string sourceName,
        string? categoryName,
        BillingCycleItemInclusionSource inclusionSource) =>
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
            inclusionSource,
            t.CreatedAt);

    /// <summary>Maps an installment pay line on a billing-cycle statement.</summary>
    public static FinBillingCycleInstallmentDueDto ToInstallmentDueDto(
        FinInstallmentPay pay,
        Guid originalTxnId,
        int totalInstallments,
        string planDescription,
        string? categoryName) =>
        new(
            pay.Id,
            pay.PlanId,
            originalTxnId,
            planDescription,
            categoryName,
            pay.InstallmentNumber,
            totalInstallments,
            pay.DueDate,
            CurrencyUnits.ToWhole(pay.Amount),
            CurrencyUnits.ToWhole(pay.PaidAmount),
            pay.Status);
}
