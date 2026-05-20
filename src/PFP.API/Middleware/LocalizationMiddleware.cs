using Microsoft.EntityFrameworkCore;
using PFP.Application.Common.Localization;
using PFP.Infrastructure.Persistence;

namespace PFP.API.Middleware;

/// <summary>Parses <c>Accept-Language</c>, picks an active locale code, stores it in <see cref="RequestLocalizationKeys.HttpContextLocaleItemKey"/>.</summary>
public sealed class LocalizationMiddleware
{
    private const string FallbackLocale = "vi";
    private static readonly TimeSpan LocaleQueryTimeout = TimeSpan.FromSeconds(5);

    private readonly RequestDelegate _next;
    private readonly ILogger<LocalizationMiddleware> _logger;

    /// <summary>Creates the middleware.</summary>
    public LocalizationMiddleware(RequestDelegate next, ILogger<LocalizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the next pipeline stage.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldBypass(context.Request.Path))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var dbFactory = context.RequestServices.GetRequiredService<IDbContextFactory<AppDbContext>>();

        string selected;
        try
        {
            selected = await ResolveLocaleAsync(context, dbFactory).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Locale resolution failed for {Method} {Path}; using fallback '{Fallback}'.",
                context.Request.Method,
                context.Request.Path,
                FallbackLocale);
            selected = FallbackLocale;
        }

        context.Items[RequestLocalizationKeys.HttpContextLocaleItemKey] = selected;

        await _next(context).ConfigureAwait(false);
    }

    private static async Task<string> ResolveLocaleAsync(
        HttpContext context,
        IDbContextFactory<AppDbContext> dbFactory)
    {
        // Factory creates a short-lived context (released before controller/MediatR handlers run).
        await using var db = await dbFactory.CreateDbContextAsync(context.RequestAborted).ConfigureAwait(false);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        timeoutCts.CancelAfter(LocaleQueryTimeout);

        var active = await db.Locales.AsNoTracking()
            .Where(l => l.IsActive)
            .OrderByDescending(l => l.IsDefault)
            .Select(l => new { l.Code, l.IsDefault })
            .ToListAsync(timeoutCts.Token)
            .ConfigureAwait(false);

        var defaultCode = active.FirstOrDefault(a => a.IsDefault)?.Code
            ?? active.FirstOrDefault()?.Code
            ?? FallbackLocale;

        var activeCodes = new HashSet<string>(active.Select(a => a.Code), StringComparer.OrdinalIgnoreCase);

        return PickLocale(context.Request.Headers.AcceptLanguage.ToString(), activeCodes, defaultCode);
    }

    private static bool ShouldBypass(PathString path) =>
        path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/api/v1/auth", StringComparison.OrdinalIgnoreCase);

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
