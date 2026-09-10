using System.Text.Json;

namespace PFP.Application.Features.Budgets.Common;

internal static class BudgetThresholdJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(IReadOnlyList<decimal> thresholds) =>
        JsonSerializer.Serialize(thresholds, Options);

    public static IReadOnlyList<decimal> Deserialize(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? Array.Empty<decimal>()
            : JsonSerializer.Deserialize<decimal[]>(json, Options) ?? Array.Empty<decimal>();
}
