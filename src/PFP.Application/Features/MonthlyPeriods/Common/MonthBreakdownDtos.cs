using PFP.Domain.Enums;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>One slice of the expense breakdown (legacy top-N shape).</summary>
public sealed record CategoryAmountBreakdownDto(Guid CategoryId, string CategoryName, long Amount);

/// <summary>One category slice for month reports / close-month JSON.</summary>
public sealed record MonthCategoryBreakdownItemDto(
    Guid? CategoryId,
    string CategoryName,
    long Amount,
    int TransactionCount,
    decimal PercentageOfTotalExpense);

/// <summary>Per-source expense totals for a month.</summary>
public sealed record MonthSourceBreakdownItemDto(Guid SourceId, string SourceName, long ExpenseAmount);

/// <summary>One day in a monthly cashflow grid.</summary>
public sealed record DailyCashflowDto(DateOnly Date, long Income, long Expense);

/// <summary>Large transaction row for report highlights.</summary>
public sealed record MonthlyReportTopTransactionDto(
    Guid Id,
    TransactionType Type,
    long Amount,
    string Description,
    DateOnly TxnDate,
    string? CategoryName,
    string SourceName);

/// <summary>Month-over-month % deltas (null when prior amount is 0).</summary>
public sealed record MonthOverMonthComparisonDto(
    decimal? IncomeChangePercent,
    decimal? ExpenseChangePercent,
    decimal? NetChangePercent);

/// <summary>Direct (cash/bank) expense line in a monthly report.</summary>
public sealed record MonthlyReportDirectExpenseItemDto(
    Guid Id,
    long Amount,
    string Currency,
    DateOnly TxnDate,
    string Description,
    string? CategoryName,
    string SourceName);

/// <summary>Direct expenses paid in the calendar month.</summary>
public sealed record MonthlyReportDirectExpenseSectionDto(
    long TotalAmount,
    int TransactionCount,
    IReadOnlyList<MonthlyReportDirectExpenseItemDto> Items);

/// <summary>Deferred transaction on a billing-cycle statement in the report.</summary>
public sealed record MonthlyReportBillingCycleTxnItemDto(
    Guid Id,
    long Amount,
    DateOnly TxnDate,
    string Description,
    string? CategoryName);

/// <summary>Installment pay line due in the billing cycle statement month.</summary>
public sealed record MonthlyReportBillingCycleInstallmentDueDto(
    Guid PayId,
    Guid PlanId,
    string PlanDescription,
    string? CategoryName,
    int InstallmentNumber,
    int TotalInstallments,
    DateOnly DueDate,
    long Amount,
    long PaidAmount,
    InstallmentPayStatus Status);

/// <summary>One billing cycle whose statement falls in the report month.</summary>
public sealed record MonthlyReportBillingCycleItemDto(
    Guid Id,
    Guid SourceId,
    string SourceName,
    string Name,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly StatementDate,
    DateOnly PaymentDueDate,
    long TotalAmount,
    long PaidAmount,
    BillingCycleStatus Status,
    IReadOnlyList<MonthlyReportBillingCycleTxnItemDto> Transactions,
    IReadOnlyList<MonthlyReportBillingCycleInstallmentDueDto> InstallmentDues);

/// <summary>Billing cycles with statement date in the report month.</summary>
public sealed record MonthlyReportBillingCyclesSectionDto(
    long TotalAmount,
    int CycleCount,
    IReadOnlyList<MonthlyReportBillingCycleItemDto> Cycles);
