using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PFP.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Helpers that turn an <see cref="EntityEntry"/> into the JSON shapes consumed by
/// <c>AUDIT_LOGS</c> and <c>*_HISTORY</c> tables (spec §3.3, §3.7).
/// <para>
/// Honours the "never log secrets" rule of spec §8 by hard-redacting a fixed set of property names
/// that we know carry credentials, tokens, or signed URLs. The matcher also catches any property
/// whose name contains <c>password</c>, <c>secret</c>, or <c>token</c> (case-insensitive) so a new
/// sensitive column is redacted by default rather than by remembering to add it here.
/// </para>
/// </summary>
internal static class EntitySnapshot
{
    /// <summary>Exact property names that must never appear in audit / history snapshots.</summary>
    private static readonly HashSet<string> ExactRedactedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "DownloadUrl",
    };

    /// <summary>Substrings whose presence in a property name triggers redaction (per spec §8).</summary>
    private static readonly string[] RedactedSubstrings = { "password", "secret", "token" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        // Render enums as member name (e.g. "DebtBorrow") rather than the underlying int — more
        // useful for incident response. The DB-level snake_case representation is handled by the
        // EF value converter (SnakeCaseEnumConverter); the JSON snapshot is forensic, not auth-of-truth.
        Converters = { new JsonStringEnumConverter() },
    };

    private static bool IsRedacted(string propertyName)
    {
        if (ExactRedactedProperties.Contains(propertyName)) return true;
        foreach (var needle in RedactedSubstrings)
        {
            if (propertyName.Contains(needle, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>
    /// Builds a JSON object containing every scalar property on the entry. When <paramref name="useOriginal"/>
    /// is <c>true</c>, original values are used (suitable for <c>BeforeSnapshot</c>); otherwise current values
    /// (suitable for <c>AfterSnapshot</c> and version history).
    /// </summary>
    public static string Snapshot(EntityEntry entry, bool useOriginal)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty()) continue;
            var name = property.Metadata.Name;
            if (IsRedacted(name))
            {
                dict[name] = "[REDACTED]";
                continue;
            }
            dict[name] = useOriginal ? property.OriginalValue : property.CurrentValue;
        }
        return JsonSerializer.Serialize(dict, JsonOptions);
    }

    /// <summary>
    /// Returns a JSON array of property names whose <c>OriginalValue</c> differs from <c>CurrentValue</c>.
    /// Used to populate <c>changed_fields</c> on update rows.
    /// </summary>
    public static string ChangedFields(EntityEntry entry)
    {
        var list = new List<string>();
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty()) continue;
            if (!property.IsModified) continue;
            if (Equals(property.OriginalValue, property.CurrentValue)) continue;
            list.Add(property.Metadata.Name);
        }
        return JsonSerializer.Serialize(list, JsonOptions);
    }
}
