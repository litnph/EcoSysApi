using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>Shared business rules for installment plans.</summary>
public static class InstallmentPlanRules
{
    /// <summary>Display label from the original transaction category, falling back to description.</summary>
    public static string ResolveOriginalTxnTitle(FinInstallmentPlan plan) =>
        plan.OriginalTransaction.Category?.Name?.Trim()
        ?? plan.OriginalTransaction.Description?.Trim()
        ?? "Trả góp";

    /// <summary>Whether the plan may be hard-deleted through the API.</summary>
    public static bool CanDelete(FinInstallmentPlan plan)
    {
        if (plan.ConversionFeeStatus is ConversionFeeStatus.Billed or ConversionFeeStatus.Paid)
            return false;

        if (plan.Status == InstallmentStatus.Active)
            return !plan.Pays.Any(p => p.TxnId is not null);

        if (plan.Status == InstallmentStatus.Completed)
            return true;

        return false;
    }
}
