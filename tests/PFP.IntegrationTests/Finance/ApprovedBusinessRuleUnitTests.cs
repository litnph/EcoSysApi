using PFP.Application.Common;
using PFP.Application.Common.Exceptions;
using PFP.Application.Features.BillingCycles.Common;
using PFP.Application.Features.Budgets.Common;
using PFP.Application.Features.InstallmentPlans.Common;
using PFP.Application.Features.Sources.CreateSource;
using PFP.Application.Features.Sources.UpdateSource;
using PFP.Application.Features.Transactions.CreateTransaction;
using PFP.Application.Features.Transactions.ImportTransactions;
using PFP.Domain.Entities.Finance;
using PFP.Domain.Enums;
using Xunit;

namespace PFP.IntegrationTests.Finance;

public sealed class ApprovedBusinessRuleUnitTests
{
    [Theory]
    [InlineData(1, 2026, 8, 2026, 7, 1, 2026, 8, 1)]
    [InlineData(20, 2026, 8, 2026, 7, 20, 2026, 8, 20)]
    public void Reporting_period_uses_inclusive_previous_and_exclusive_current_cutoff(
        int reportDay,
        int year,
        int month,
        int startYear,
        int startMonth,
        int startDay,
        int endYear,
        int endMonth,
        int endDay)
    {
        var period = ReportingPeriodCalculator.ForTargetMonth(year, month, reportDay);

        Assert.Equal(new DateOnly(startYear, startMonth, startDay), period.StartInclusive);
        Assert.Equal(new DateOnly(endYear, endMonth, endDay), period.EndExclusive);
        Assert.True(period.Contains(period.StartInclusive));
        Assert.False(period.Contains(period.EndExclusive));
    }

    [Theory]
    [InlineData(2023, 3, 2023, 2, 28)]
    [InlineData(2024, 3, 2024, 2, 29)]
    public void Reporting_day_31_clamps_each_February_boundary(
        int year,
        int month,
        int startYear,
        int startMonth,
        int startDay)
    {
        var period = ReportingPeriodCalculator.ForTargetMonth(year, month, 31);

        Assert.Equal(new DateOnly(startYear, startMonth, startDay), period.StartInclusive);
        Assert.Equal(new DateOnly(year, month, 31), period.EndExclusive);
    }

    [Fact]
    public void Reporting_period_handles_December_to_January_transition()
    {
        var period = ReportingPeriodCalculator.ForTargetMonth(2027, 1, 31);

        Assert.Equal(new DateOnly(2026, 12, 31), period.StartInclusive);
        Assert.Equal(new DateOnly(2027, 1, 31), period.EndExclusive);
    }

    [Fact]
    public void Reporting_midnight_converts_with_the_finance_timezone()
    {
        var boundary = ReportingPeriodCalculator.ToUtcBoundary(new DateOnly(2026, 7, 20));

        Assert.Equal(DateTimeOffset.Parse("2026-07-19T17:00:00Z"), boundary);
    }

    [Theory]
    [InlineData(2026, 8, 19, 20, 2026, 8)]
    [InlineData(2026, 8, 20, 20, 2026, 9)]
    [InlineData(2023, 2, 27, 31, 2023, 2)]
    [InlineData(2023, 2, 28, 31, 2023, 3)]
    public void Active_reporting_cycle_uses_the_month_containing_its_end_boundary(
        int year,
        int month,
        int day,
        int reportDay,
        int expectedYear,
        int expectedMonth)
    {
        var target = ReportingPeriodCalculator.TargetMonthContaining(
            new DateOnly(year, month, day),
            reportDay);

        Assert.Equal((expectedYear, expectedMonth), target);
    }

    [Theory]
    [InlineData(0, new double[0])]
    [InlineData(1, new[] { 50d })]
    [InlineData(2, new[] { 33.33d, 66.67d })]
    [InlineData(3, new[] { 25d, 50d, 75d })]
    [InlineData(4, new[] { 20d, 40d, 60d, 80d })]
    public void Warning_thresholds_are_evenly_distributed(int count, double[] expected)
    {
        Assert.Equal(expected.Select(value => (decimal)value), BudgetThresholds.Generate(count));
    }

    [Fact]
    public void Warning_threshold_validation_rejects_duplicates_and_out_of_range_values()
    {
        Assert.False(BudgetThresholds.IsValid(new[] { 25m, 25m }, 2));
        Assert.False(BudgetThresholds.IsValid(new[] { 0m }, 1));
        Assert.False(BudgetThresholds.IsValid(new[] { 100m }, 1));
        Assert.False(BudgetThresholds.IsValid(new[] { 75m, 25m }, 2));
        Assert.True(BudgetThresholds.IsValid(new[] { 25m, 75m }, 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(61)]
    public void Credit_card_due_offset_outside_one_to_sixty_is_rejected(int dueDays)
    {
        var create = new CreateSourceCommand(
            "Card", SourceType.CreditCard, 10_000m, 15, dueDays, null,
            "VND", null, null, null, null);
        var update = new UpdateSourceCommand(
            Guid.NewGuid(), "Card", SourceType.CreditCard, 10_000m, 15, dueDays,
            null, "VND", null, null, null);

        Assert.Contains(new CreateSourceCommandValidator().Validate(create).Errors,
            error => error.PropertyName == nameof(CreateSourceCommand.PaymentDueDay));
        Assert.Contains(new UpdateSourceCommandValidator().Validate(update).Errors,
            error => error.PropertyName == nameof(UpdateSourceCommand.PaymentDueDay));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(60)]
    public void Credit_card_due_offset_boundaries_are_accepted(int dueDays)
    {
        var command = new CreateSourceCommand(
            "Card", SourceType.CreditCard, 10_000m, 15, dueDays, null,
            "VND", null, null, null, null);

        Assert.DoesNotContain(new CreateSourceCommandValidator().Validate(command).Errors,
            error => error.PropertyName == nameof(CreateSourceCommand.PaymentDueDay));
    }

    [Fact]
    public void Past_installment_is_overdue_and_never_implicitly_paid()
    {
        var pay = new FinInstallmentPay();
        var today = new DateOnly(2026, 7, 30);

        InstallmentPaySchedule.ApplyInitialPayLine(
            pay,
            1_000m,
            today.AddDays(-10),
            today.AddDays(-1),
            today);

        Assert.Equal(InstallmentPayStatus.Overdue, pay.Status);
        Assert.Equal(0m, pay.PaidAmount);
        Assert.Null(pay.PaidAt);
        Assert.Null(pay.TxnId);
    }

    [Theory]
    [InlineData(2026, 8, 9, 15, 1, 2026, 8, 15)]
    [InlineData(2026, 8, 15, 15, 1, 2026, 9, 15)]
    [InlineData(2026, 1, 30, 31, 1, 2026, 1, 31)]
    [InlineData(2026, 1, 30, 31, 2, 2026, 2, 28)]
    [InlineData(2026, 1, 30, 31, 3, 2026, 3, 31)]
    public void Installment_schedule_follows_credit_card_statement_dates(
        int txnYear,
        int txnMonth,
        int txnDay,
        int statementDay,
        int installmentNumber,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var statementDate = InstallmentPaySchedule.StatementDateForInstallment(
            new DateOnly(txnYear, txnMonth, txnDay),
            statementDay,
            installmentNumber);

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), statementDate);
    }

    [Fact]
    public void Installment_due_date_is_the_statement_date_plus_card_due_offset()
    {
        var dueDate = InstallmentPaySchedule.DueDateForInstallment(
            new DateOnly(2026, 8, 9),
            statementDay: 15,
            paymentDueDaysAfterStatement: 25,
            installmentNumber: 1);

        Assert.Equal(new DateOnly(2026, 9, 9), dueDate);
    }

    [Fact]
    public void Installment_is_attached_to_the_same_statement_month_even_if_day_changed()
    {
        var pay = new FinInstallmentPay
        {
            StatementDate = new DateOnly(2026, 8, 15),
        };

        Assert.True(BillingCycleInstallmentRules.IsPayInStatementMonth(
            pay,
            new DateOnly(2026, 8, 20)));
        Assert.False(BillingCycleInstallmentRules.IsPayInStatementMonth(
            pay,
            new DateOnly(2026, 9, 15)));
    }

    [Fact]
    public void Installment_status_is_derived_on_read_without_overwriting_payment_evidence()
    {
        var today = new DateOnly(2026, 8, 9);

        Assert.Equal(
            InstallmentPayStatus.Overdue,
            InstallmentPaySchedule.ResolveStatus(
                InstallmentPayStatus.Upcoming,
                today.AddDays(-1),
                today));
        Assert.Equal(
            InstallmentPayStatus.Due,
            InstallmentPaySchedule.ResolveStatus(
                InstallmentPayStatus.Upcoming,
                today,
                today));
        Assert.Equal(
            InstallmentPayStatus.Paid,
            InstallmentPaySchedule.ResolveStatus(
                InstallmentPayStatus.Paid,
                today.AddYears(1),
                today));
    }

    [Fact]
    public void Installment_split_preserves_whole_principal()
    {
        var (monthly, last) = InstallmentScheduleSplit.Split(10_000m, 3);

        Assert.Equal(10_000m, monthly * 2 + last);
        Assert.True(last >= 0m);
    }

    [Theory]
    [InlineData("2026-07-30T16:59:59Z", 2026, 7, 30)]
    [InlineData("2026-07-30T17:00:00Z", 2026, 7, 31)]
    [InlineData("2026-07-31T17:00:00Z", 2026, 8, 1)]
    public void Finance_calendar_uses_Asia_Bangkok_midnight(
        string utcInstant,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var actual = FinanceBusinessCalendar.GetDate(DateTimeOffset.Parse(utcInstant));

        Assert.Equal(new DateOnly(expectedYear, expectedMonth, expectedDay), actual);
        Assert.Equal("Asia/Bangkok", FinanceBusinessCalendar.TimeZoneId);
    }

    [Fact]
    public void Optimistic_concurrency_rejects_a_stale_expected_version()
    {
        Assert.Throws<ConcurrencyConflictException>(() =>
            OptimisticConcurrencyGuard.Ensure(actualVersion: 4, expectedVersion: 3));
        OptimisticConcurrencyGuard.Ensure(actualVersion: 4, expectedVersion: 4);
        OptimisticConcurrencyGuard.Ensure(actualVersion: 4, expectedVersion: null);
    }

    [Fact]
    public void Import_requires_stable_unique_row_keys()
    {
        var repeated = Guid.NewGuid();
        var command = new PreviewTransactionImportCommand(new[]
        {
            ImportRow(repeated),
            ImportRow(repeated),
        });

        var result = new PreviewTransactionImportCommandValidator().Validate(command);

        Assert.Contains(result.Errors, error =>
            error.ErrorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Import_batch_limit_is_enforced()
    {
        var rows = Enumerable.Range(0, 101).Select(_ => ImportRow(Guid.NewGuid())).ToArray();

        var result = new CommitTransactionImportCommandValidator()
            .Validate(new CommitTransactionImportCommand(rows));

        Assert.Contains(result.Errors, error =>
            error.ErrorMessage.Contains("between 1 and 100", StringComparison.Ordinal));
    }

    private static CreateTransactionCommand ImportRow(Guid clientRequestId) =>
        new(
            TransactionType.Direct,
            1_000,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 7, 30),
            null,
            "Import row",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            clientRequestId);
}
