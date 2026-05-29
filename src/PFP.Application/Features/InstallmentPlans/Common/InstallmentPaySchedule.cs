using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>Due dates and initial status for installment pay lines at plan creation.</summary>
public static class InstallmentPaySchedule
{
    /// <summary>Due date for installment <paramref name="installmentNumber"/> (1-based) from plan start.</summary>
    public static DateOnly DueDateForInstallment(DateOnly startDate, int installmentNumber) =>
        startDate.AddMonths(installmentNumber - 1);

    /// <summary>
    /// Past periods (<paramref name="dueDate"/> before <paramref name="today"/>) are marked paid for backfill.
    /// Today is due; future periods are upcoming.
    /// </summary>
    public static void ApplyInitialPayLine(
        FinInstallmentPay pay,
        decimal amount,
        DateOnly dueDate,
        DateOnly today)
    {
        pay.Amount = amount;
        pay.DueDate = dueDate;

        if (dueDate < today)
        {
            pay.Status = InstallmentPayStatus.Paid;
            pay.PaidAmount = amount;
            pay.PaidAt = dueDate.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc);
            pay.TxnId = null;
            return;
        }

        pay.PaidAmount = 0;
        pay.PaidAt = null;
        pay.TxnId = null;
        pay.Status = dueDate == today
            ? InstallmentPayStatus.Due
            : InstallmentPayStatus.Upcoming;
    }

    /// <summary>True when every pay line is already marked paid (e.g. full backfill).</summary>
    public static bool IsFullyPaid(IEnumerable<FinInstallmentPay> pays) =>
        pays.All(p => p.Status == InstallmentPayStatus.Paid);
}
