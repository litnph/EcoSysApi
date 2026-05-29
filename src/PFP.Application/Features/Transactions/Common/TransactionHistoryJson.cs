using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using PFP.Domain.Entities;

namespace PFP.Application.Features.Transactions.Common;

/// <summary>Builds the JSON snapshot stored on the first <see cref="FinTransactionHistory"/> row at create time.</summary>
internal static class TransactionHistoryJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Serialises the transactional fields (no navigation graphs).</summary>
    public static string BuildCreatedSnapshot(FinTransaction t) =>
        JsonSerializer.Serialize(BuildStateAnonymous(t), Options);

    /// <summary>Serialises the current scalar state including soft-delete flags (audit / deleted history).</summary>
    public static string BuildTransactionStateSnapshot(FinTransaction t) =>
        JsonSerializer.Serialize(BuildStateAnonymous(t), Options);

    private static object BuildStateAnonymous(FinTransaction t) =>
        new
        {
            t.Id,
            t.Type,
            t.Status,
            t.Amount,
            t.Currency,
            txnDate = t.TxnDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            t.SourceId,
            t.DestSourceId,
            t.CategoryId,
            t.MonthlyPeriodId,
            t.RefTxnId,
            t.Description,
            t.Note,
            t.Version,
            t.IsDeleted,
            deletedAt = t.DeletedAt.HasValue
                ? t.DeletedAt.Value.ToString("O", CultureInfo.InvariantCulture)
                : null,
            t.DeletedBy,
        };
}
