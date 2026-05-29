using System.Text.Json;
using System.Text.Json.Serialization;

namespace PFP.Application.Features.MonthlyPeriods.Common;

/// <summary>Serialises and deserialises <see cref="MonthlyReportDto"/> for <c>fin_monthly_periods.report_snapshot</c>.</summary>
internal static class MonthlyReportSnapshotStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string Serialize(MonthlyReportDto report) =>
        JsonSerializer.Serialize(report, Options);

    public static MonthlyReportDto Deserialize(string json) =>
        JsonSerializer.Deserialize<MonthlyReportDto>(json, Options)
        ?? throw new InvalidOperationException("Report snapshot JSON is invalid.");
}
