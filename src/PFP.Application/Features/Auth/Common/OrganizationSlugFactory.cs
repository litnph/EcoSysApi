using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;

namespace PFP.Application.Features.Auth.Common;

/// <summary>Builds globally-unique URL slugs for auto-created personal organisations.</summary>
internal static partial class OrganizationSlugFactory
{
    /// <summary>Reserves a unique slug against <c>ORGANIZATIONS.slug</c>.</summary>
    internal static async Task<string> ReserveUniqueSlugAsync(
        IApplicationDbContext db,
        string fullName,
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(fullName);
        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "personal";

        var suffix = ownerUserId.ToString("N", CultureInfo.InvariantCulture)[..8];
        var candidate = $"{baseSlug}-{suffix}";
        candidate = candidate.Length <= 64 ? candidate : candidate[..64];

        var unique = candidate;
        var n = 0;
        while (await db.Organizations.AnyAsync(o => o.Slug == unique, cancellationToken).ConfigureAwait(false))
        {
            n++;
            var extra = $"-{n}";
            var max = 64 - extra.Length;
            unique = (candidate.Length <= max ? candidate : candidate[..max]) + extra;
        }

        return unique;
    }

    private static string Slugify(string input)
    {
        var lower = input.Trim().ToLowerInvariant();
        var noApostrophe = lower.Replace('\'', '-');
        var dashed = SlugInvalidChars().Replace(noApostrophe, "-");
        while (dashed.Contains("--", StringComparison.Ordinal))
            dashed = dashed.Replace("--", "-", StringComparison.Ordinal);
        return dashed.Trim('-');
    }

    [GeneratedRegex("[^a-z0-9-]+", RegexOptions.CultureInvariant)]
    private static partial Regex SlugInvalidChars();
}
