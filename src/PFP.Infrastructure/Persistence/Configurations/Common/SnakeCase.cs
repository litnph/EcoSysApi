using System.Text;

namespace PFP.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Tiny helpers for converting CLR PascalCase names into PostgreSQL-friendly snake_case identifiers.
/// <para>
/// Used by the model naming convention to map entities like <c>FinTransaction</c> → <c>fin_transactions</c>
/// and properties like <c>BillingCycleId</c> → <c>billing_cycle_id</c>, and by the enum value converter
/// so that <see cref="Domain.Enums.TransactionType.DebtBorrow"/> is stored as <c>debt_borrow</c>
/// (spec §3.6 — "the database column stores the snake_case string").
/// </para>
/// </summary>
internal static class SnakeCase
{
    /// <summary>
    /// Converts <c>FinBillingCycle</c> → <c>fin_billing_cycle</c>, <c>HTTPHeader</c> → <c>http_header</c>.
    /// Preserves runs of capitals and digits.
    /// </summary>
    public static string From(string pascal)
    {
        if (string.IsNullOrEmpty(pascal)) return pascal;
        var sb = new StringBuilder(pascal.Length + 8);
        for (int i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    var prev = pascal[i - 1];
                    var next = i + 1 < pascal.Length ? pascal[i + 1] : '\0';
                    var startOfWord = !char.IsUpper(prev) && prev != '_';
                    var endOfAcronym = char.IsUpper(prev) && next != '\0' && char.IsLower(next);
                    if (startOfWord || endOfAcronym) sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>Parses a snake_case string back into PascalCase, e.g. <c>debt_borrow</c> → <c>DebtBorrow</c>.</summary>
    public static string ToPascal(string snake)
    {
        if (string.IsNullOrEmpty(snake)) return snake;
        var sb = new StringBuilder(snake.Length);
        var upperNext = true;
        foreach (var c in snake)
        {
            if (c == '_')
            {
                upperNext = true;
                continue;
            }
            sb.Append(upperNext ? char.ToUpperInvariant(c) : c);
            upperNext = false;
        }
        return sb.ToString();
    }
}
