using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Interfaces;
using PFP.Application.Common.Localization;

namespace PFP.API.Middleware;

/// <summary>Parses <c>Accept-Language</c>, picks an active locale code, stores it in <see cref="RequestLocalizationKeys.HttpContextLocaleItemKey"/>.</summary>
public sealed class LocalizationMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public LocalizationMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Invokes the next pipeline stage.</summary>
    public async Task InvokeAsync(HttpContext context, IServiceScopeFactory scopeFactory)
    {
        if (ShouldBypass(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Resolve locale in a short-lived scope so the DbContext (and its pooled connection)
        // is released before controller/MediatR handlers run — avoids starving Neon pooler slots.
        string selected;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var active = await db.Locales.AsNoTracking()
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.IsDefault)
                .Select(l => new { l.Code, l.IsDefault })
                .ToListAsync(context.RequestAborted)
                .ConfigureAwait(false);

            var defaultCode = active.FirstOrDefault(a => a.IsDefault)?.Code
                ?? active.FirstOrDefault()?.Code
                ?? "vi";

            var activeCodes = new HashSet<string>(active.Select(a => a.Code), StringComparer.OrdinalIgnoreCase);

            selected = PickLocale(context.Request.Headers.AcceptLanguage.ToString(), activeCodes, defaultCode);
        }

        context.Items[RequestLocalizationKeys.HttpContextLocaleItemKey] = selected;

        await _next(context).ConfigureAwait(false);
    }

    private static bool ShouldBypass(PathString path) =>
        path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase);

    private static string PickLocale(string? acceptLanguage, HashSet<string> activeCodes, string defaultCode)
    {
        if (activeCodes.Count == 0 || string.IsNullOrWhiteSpace(acceptLanguage))
            return defaultCode;

        foreach (var part in acceptLanguage.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var tag = part.Split(';', 2)[0].Trim();
            if (tag.Length == 0)
                continue;

            foreach (var code in activeCodes)
            {
                if (string.Equals(code, tag, StringComparison.OrdinalIgnoreCase))
                    return code;
            }

            var primary = tag.Split('-')[0];
            foreach (var code in activeCodes)
            {
                if (string.Equals(code, primary, StringComparison.OrdinalIgnoreCase))
                    return code;
            }
        }

        return defaultCode;
    }
}
