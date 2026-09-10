using System.Globalization;
using System.Text;
using PFP.Domain.Entities;

namespace PFP.Application.Features.TransactionClassificationRules;

/// <summary>Pure, deterministic keyword matching used by preview and commit flows.</summary>
public static class TransactionContentMatcher
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var source = value.Trim().Normalize(NormalizationForm.FormD);
        var output = new StringBuilder(source.Length);
        var previousWhitespace = false;
        foreach (var character in source)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                continue;

            var normalized = character is 'đ' or 'Đ' ? 'D' : char.ToUpperInvariant(character);
            if (char.IsWhiteSpace(normalized))
            {
                if (!previousWhitespace) output.Append(' ');
                previousWhitespace = true;
            }
            else
            {
                output.Append(normalized);
                previousWhitespace = false;
            }
        }

        return output.ToString().Trim().Normalize(NormalizationForm.FormC);
    }

    public static bool IsMatch(string? content, string normalizedKeyword)
    {
        var normalizedContent = Normalize(content);
        if (normalizedContent.Length == 0 || normalizedKeyword.Length == 0) return false;

        var start = 0;
        while ((start = normalizedContent.IndexOf(normalizedKeyword, start, StringComparison.Ordinal)) >= 0)
        {
            var end = start + normalizedKeyword.Length;
            var leftOk = !char.IsLetterOrDigit(normalizedKeyword[0])
                         || start == 0
                         || !char.IsLetterOrDigit(normalizedContent[start - 1]);
            var rightOk = !char.IsLetterOrDigit(normalizedKeyword[^1])
                          || end == normalizedContent.Length
                          || !char.IsLetterOrDigit(normalizedContent[end]);
            if (leftOk && rightOk) return true;
            start++;
        }

        return false;
    }

    public static TransactionClassificationRule? SelectBest(
        IEnumerable<TransactionClassificationRule> rules,
        string? content) => rules
        .Where(rule => rule.IsActive && IsMatch(content, rule.NormalizedKeyword))
        .OrderByDescending(rule => rule.NormalizedKeyword.Length)
        .ThenBy(rule => rule.CreatedAt)
        .ThenBy(rule => rule.Id)
        .FirstOrDefault();
}
