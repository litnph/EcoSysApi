using System.Net.Http;
using System.Text.Json;

namespace PFP.IntegrationTests.Finance;

/// <summary>JSON body for <c>POST /api/v1/finance/transactions</c> (camelCase via <see cref="JsonSerializerDefaults.Web"/>).</summary>
internal sealed record CreateTransactionWire(
    Guid SmoduleId,
    string Type,
    long Amount,
    Guid SourceId,
    Guid? CategoryId,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId,
    Guid? ToSourceId,
    IReadOnlyList<SplitItemWire>? Splits = null,
    Guid? BillingCycleId = null,
    string? PersonName = null,
    string? PersonContact = null,
    Guid? DebtRecordId = null,
    DateOnly? DueDate = null);

internal sealed record SplitItemWire(string PersonName, string? PersonContact, long Amount);

/// <summary>Mirrors <see cref="PFP.Application.Features.BillingCycles.Commands.GenerateBillingCycle.GenerateBillingCycleCommandHandler"/> date math.</summary>
internal static class BillingCycleDateMath
{
    internal static DateOnly DayInMonth(int year, int month, int dayOfMonth)
    {
        var last = DateTime.DaysInMonth(year, month);
        var day = dayOfMonth > last ? last : dayOfMonth;
        return new DateOnly(year, month, day);
    }

    internal static (DateOnly PeriodStart, DateOnly PeriodEnd, DateOnly StatementDate, DateOnly PaymentDueDate) ExpectedForStatementDay(
        int statementDay,
        int paymentDueDay)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var prev = today.AddMonths(-1);
        var periodStart = DayInMonth(prev.Year, prev.Month, statementDay);
        var statementDate = DayInMonth(today.Year, today.Month, statementDay);
        var periodEnd = statementDate.AddDays(-1);
        var paymentDueDate = statementDate.AddDays(paymentDueDay);
        return (periodStart, periodEnd, statementDate, paymentDueDate);
    }
}

internal static class FinanceApiWireJson
{
    internal static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    internal static async Task<Guid> ReadTransactionIdFromCreateResponseAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("data").GetProperty("transaction").GetProperty("id").GetGuid();
    }

    internal static async Task<Guid> ReadBillingCycleIdFromGenerateResponseAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("data").GetProperty("cycle").GetProperty("id").GetGuid();
    }

    internal static async Task<Guid> ReadInstallmentPlanIdFromCreateResponseAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("data").GetProperty("planId").GetGuid();
    }

    internal static async Task<string?> ReadFirstBusinessRuleMessageAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("error").GetProperty("messages")[0].GetString();
    }
}
