using Microsoft.EntityFrameworkCore;

namespace PFP.Infrastructure.Persistence;

/// <summary>Clears all finance-related rows (keeps users, locales, UI strings).</summary>
public static class DemoFinanceDataReset
{
    public static async Task ClearAllFinanceDataAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.EntityTags.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinTransactionHistory.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinTxnSplits.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinDebtTransactions.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinDebtRecordHistory.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinInstallmentPays.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinInstallmentPlanHistory.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinInvestmentTxns.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        await db.FinTransactions
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RefTxnId, (Guid?)null),
                cancellationToken)
            .ConfigureAwait(false);
        await db.FinTransactions.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        await db.FinInstallmentPlans.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinDebtRecords.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinBillingCycles.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinSavings.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinInvestments.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinSourceHistory.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinMonthlyPeriods.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinCategories.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.FinSources.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        await db.Tags.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        await db.FileAttachments
            .Where(f => f.EntityType == "FinTransaction" || f.EntityType == "FinSource")
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        await db.AuditLogs
            .Where(a => a.EntityType != null && (
                a.EntityType.StartsWith("Fin") ||
                a.EntityType == "Tag" ||
                a.EntityType == "EntityTag"))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
