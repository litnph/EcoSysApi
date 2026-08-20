using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;

namespace PFP.Application.Features.InstallmentPlans.Common;

/// <summary>Due dates and initial status for installment pay lines at plan creation.</summary>
public static class InstallmentPaySchedule
{
    /// <summary>The schedule formula currently written by the application.</summary>
    public const int CurrentScheduleVersion = 2;

    /// <summary>
    /// Returns the statement date that first captures a transaction. A transaction made on the
    /// statement date belongs to the following cycle because billing periods start on that date.
    /// </summary>
    public static DateOnly FirstStatementDate(DateOnly transactionDate, int statementDay)
    {
        ValidateStatementDay(statementDay);

        var candidate = DayInMonth(transactionDate.Year, transactionDate.Month, statementDay);
        if (transactionDate < candidate)
            return candidate;

        var nextMonth = new DateOnly(transactionDate.Year, transactionDate.Month, 1).AddMonths(1);
        return DayInMonth(nextMonth.Year, nextMonth.Month, statementDay);
    }

    /// <summary>
    /// Statement date for a 1-based installment number. The configured statement day is clamped
    /// independently in every month so a day such as 31 returns to 31 after February.
    /// </summary>
    public static DateOnly StatementDateForInstallment(
        DateOnly transactionDate,
        int statementDay,
        int installmentNumber)
    {
        if (installmentNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(installmentNumber));

        var firstStatement = FirstStatementDate(transactionDate, statementDay);
        var targetMonth = new DateOnly(firstStatement.Year, firstStatement.Month, 1)
            .AddMonths(installmentNumber - 1);
        return DayInMonth(targetMonth.Year, targetMonth.Month, statementDay);
    }

    /// <summary>Actual payment deadline for an installment's statement.</summary>
    public static DateOnly DueDateForInstallment(
        DateOnly transactionDate,
        int statementDay,
        int paymentDueDaysAfterStatement,
        int installmentNumber)
    {
        if (paymentDueDaysAfterStatement is < 1 or > 60)
            throw new ArgumentOutOfRangeException(nameof(paymentDueDaysAfterStatement));

        return StatementDateForInstallment(transactionDate, statementDay, installmentNumber)
            .AddDays(paymentDueDaysAfterStatement);
    }

    /// <summary>
    /// Past periods are overdue, today is due, and future periods are upcoming.
    /// No line is implicitly marked paid without payment evidence.
    /// </summary>
    public static void ApplyInitialPayLine(
        FinInstallmentPay pay,
        decimal amount,
        DateOnly statementDate,
        DateOnly dueDate,
        DateOnly today)
    {
        pay.Amount = amount;
        pay.StatementDate = statementDate;
        pay.DueDate = dueDate;

        pay.PaidAmount = 0;
        pay.PaidAt = null;
        pay.TxnId = null;
        pay.Status = ResolveStatus(pay.Status, dueDate, today);
    }

    /// <summary>
    /// Resolves the time-dependent display status without mutating stored financial evidence.
    /// A paid row always remains paid; every other status is derived from its actual due date.
    /// </summary>
    public static InstallmentPayStatus ResolveStatus(
        InstallmentPayStatus storedStatus,
        DateOnly dueDate,
        DateOnly today)
    {
        if (storedStatus == InstallmentPayStatus.Paid)
            return InstallmentPayStatus.Paid;

        return dueDate < today
            ? InstallmentPayStatus.Overdue
            : dueDate == today
                ? InstallmentPayStatus.Due
                : InstallmentPayStatus.Upcoming;
    }

    /// <summary>Resolves the time-dependent status for one scheduled row.</summary>
    public static InstallmentPayStatus ResolveStatus(FinInstallmentPay pay, DateOnly today) =>
        ResolveStatus(pay.Status, pay.DueDate, today);

    /// <summary>True when every pay line has payment evidence recorded as paid.</summary>
    public static bool IsFullyPaid(IEnumerable<FinInstallmentPay> pays) =>
        pays.All(p => p.Status == InstallmentPayStatus.Paid);

    private static DateOnly DayInMonth(int year, int month, int dayOfMonth)
    {
        var day = Math.Min(dayOfMonth, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, day);
    }

    private static void ValidateStatementDay(int statementDay)
    {
        if (statementDay is < 1 or > 31)
            throw new ArgumentOutOfRangeException(nameof(statementDay));
    }
}
