using System.Net.Http;
using System.Text.Json;

namespace PFP.IntegrationTests.Finance;

/// <summary>JSON body for <c>POST /api/v1/finance/transactions</c> (camelCase via <see cref="JsonSerializerDefaults.Web"/>).</summary>
internal sealed record CreateTransactionWire(
    string Type,
    long Amount,
    Guid SourceId,
    Guid? CategoryId,
    DateOnly TxnDate,
    string? Note,
    Guid? MonthlyPeriodId,
    Guid? ToSourceId,
    IReadOnlyList<SplitItemWire>? Splits = null,
    string? PersonName = null,
    string? PersonContact = null,
    Guid? DebtRecordId = null,
    DateOnly? DueDate = null);

internal sealed record SplitItemWire(string PersonName, string? PersonContact, long Amount);

internal static class FinanceApiWireJson
{
    internal static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    internal static async Task<Guid> ReadTransactionIdFromCreateResponseAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("data").GetProperty("transaction").GetProperty("id").GetGuid();
    }
}
