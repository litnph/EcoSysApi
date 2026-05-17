using System.Text.Json;

namespace PFP.Application.Features.Automation.Common;

/// <summary>Validates JSON payloads stored on <see cref="PFP.Domain.Entities.AutomationRule"/>.</summary>
public static class AutomationPayloadValidator
{
    private static readonly HashSet<string> AllowedActionTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "pay_statement",
            "transfer_savings",
            "send_notification",
            "create_transaction",
        };

    /// <summary>Returns <c>false</c> and sets <paramref name="error"/> when <paramref name="json"/> is not a JSON array.</summary>
    public static bool IsJsonArray(string json, out string? error)
    {
        error = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                error = "Must be a JSON array.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Validates that <paramref name="actionsJson"/> is a non-empty JSON array and each element declares an allowed <c>type</c>.
    /// </summary>
    public static bool TryValidateActions(string actionsJson, out string? error)
    {
        error = null;
        if (!IsJsonArray(actionsJson, out error))
            return false;

        using var doc = JsonDocument.Parse(actionsJson);
        var arr = doc.RootElement;
        if (arr.GetArrayLength() == 0)
        {
            error = "At least one action is required.";
            return false;
        }

        var index = 0;
        foreach (var el in arr.EnumerateArray())
        {
            index++;
            if (!el.TryGetProperty("type", out var t))
            {
                error = $"Action #{index} is missing \"type\".";
                return false;
            }

            var type = t.GetString()?.Trim();
            if (string.IsNullOrEmpty(type) || !AllowedActionTypes.Contains(type))
            {
                error = $"Action #{index} has unsupported type \"{type}\".";
                return false;
            }
        }

        return true;
    }
}
